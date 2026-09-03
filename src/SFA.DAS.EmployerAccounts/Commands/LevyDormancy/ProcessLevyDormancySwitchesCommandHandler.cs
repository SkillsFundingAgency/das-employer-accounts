using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Commands.SendNotification;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Models.LevyDormancy;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerAccounts.Commands.LevyDormancy;

public class ProcessLevyDormancySwitchesCommandHandler(
    Lazy<EmployerAccountsDbContext> db,
    IMediator mediator,
    IEmployerAccountRepository accountRepository,
    IEmployerAccountTeamRepository accountTeamRepository,
    IEventPublisher eventPublisher,
    IOptions<LevyDormancyConfiguration> levyDormancyOptions,
    ICurrentDateTime currentDateTime,
    ILogger<ProcessLevyDormancySwitchesCommandHandler> logger)
    : IRequestHandler<ProcessLevyDormancySwitchesCommand, ProcessLevyDormancySwitchesResult>
{
    private const string TransitionCompleteTemplateId = "LevyDormancyTransitionComplete";

    public async Task<ProcessLevyDormancySwitchesResult> Handle(
        ProcessLevyDormancySwitchesCommand command,
        CancellationToken cancellationToken)
    {
        var configuration = levyDormancyOptions.Value;
        var result = new ProcessLevyDormancySwitchesResult();

        if (!configuration.OrchestrationEnabled)
        {
            logger.LogInformation("Levy dormancy orchestration is disabled. No switches were processed.");
            return result;
        }

        var now = currentDateTime.Now;
        var levyEmployerType = (byte)ApprenticeshipEmployerType.Levy;
        var skipInitialWarning = configuration.SkipInitialWarning;
        var candidates = await db.Value.LevyDormancyRequests
            .Where(r => r.ActionEmailSentAt == null &&
                        ((r.Status == LevyDormancyRequestStatus.InProgress &&
                          r.WarningEmailSentAt != null) ||
                         (skipInitialWarning && r.Status == LevyDormancyRequestStatus.Pending)))
            .ToListAsync(cancellationToken);

        foreach (var request in candidates)
        {
            result.RequestsProcessed++;

            if (!IsEligibleForSwitch(request, configuration, now))
            {
                result.SkippedNotYetEligible++;

                logger.LogInformation(
                    "Levy dormancy switch not yet due for account {AccountId}, request {RequestId}",
                    request.AccountId,
                    request.Id);

                continue;
            }

            var account = await db.Value.Accounts
                .AsNoTracking()
                .SingleOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken);

            if (account == null || account.ApprenticeshipEmployerType != levyEmployerType)
            {
                request.Status = LevyDormancyRequestStatus.Cancelled;
                request.UpdatedOn = now;
                result.RequestsCancelled++;

                logger.LogInformation(
                    "Cancelled LevyDormancyRequest {RequestId} for account {AccountId} because the account is no longer levy.",
                    request.Id,
                    request.AccountId);

                continue;
            }

            await accountRepository.SetAccountLevyStatus(request.AccountId, ApprenticeshipEmployerType.NonLevy);

            await eventPublisher.Publish(new ApprenticeshipEmployerTypeChangeEvent
            {
                AccountId = request.AccountId,
                ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy,
                Created = now
            });

            result.AccountsSwitched++;

            logger.LogInformation(
                "Switched account {AccountId} from Levy to NonLevy for request {RequestId}",
                request.AccountId,
                request.Id);

            var teamMembers = await accountTeamRepository.GetAccountTeamMembers(request.AccountId);

            var recipients = teamMembers
                .Where(m =>
                    m.Status == InvitationStatus.Accepted &&
                    m.Role == Role.Owner &&
                    !string.IsNullOrWhiteSpace(m.Email))
                .ToList();

            if (recipients.Count == 0)
            {
                result.SkippedNoRecipients++;
                request.Status = LevyDormancyRequestStatus.Completed;
                request.UpdatedOn = now;

                logger.LogWarning(
                    "No email addresses found to send levy dormancy transition complete for account {AccountId}, request {RequestId}",
                    request.AccountId,
                    request.Id);

                continue;
            }

            var switchDateToken = now.ToString("dd MMMM yyyy");
            var emailsSentForRequest = 0;

            foreach (var recipient in recipients)
            {
                await mediator.Send(new SendNotificationCommand
                {
                    RecipientsAddress = recipient.Email,
                    TemplateId = TransitionCompleteTemplateId,
                    Tokens = new Dictionary<string, string>
                    {
                        ["user_first_name"] = recipient.FirstName,
                        ["employer_name"] = account.Name,
                        ["switch_date"] = switchDateToken
                    }
                }, cancellationToken);

                emailsSentForRequest++;
            }

            request.ActionEmailSentAt = now;
            request.Status = LevyDormancyRequestStatus.Completed;
            request.UpdatedOn = now;
            result.EmailsSent += emailsSentForRequest;

            logger.LogInformation(
                "Sent levy dormancy transition complete for account {AccountId}, request {RequestId}, recipients {RecipientCount}",
                request.AccountId,
                request.Id,
                emailsSentForRequest);
        }

        if (candidates.Count > 0)
        {
            await db.Value.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation(
            "Levy dormancy switch orchestration completed. Processed {RequestsProcessed}, accounts switched {AccountsSwitched}, emails sent {EmailsSent}, cancelled {RequestsCancelled}, skipped not yet eligible {SkippedNotYetEligible}, skipped no recipients {SkippedNoRecipients}",
            result.RequestsProcessed,
            result.AccountsSwitched,
            result.EmailsSent,
            result.RequestsCancelled,
            result.SkippedNotYetEligible,
            result.SkippedNoRecipients);

        return result;
    }

    private static bool IsEligibleForSwitch(
        LevyDormancyRequest request,
        LevyDormancyConfiguration configuration,
        DateTime now)
    {
        if (request.WarningEmailSentAt.HasValue)
        {
            return now >= request.WarningEmailSentAt.Value.AddMonths(configuration.MonthsBetweenInitialWarningAndSwitch);
        }

        return configuration.SkipInitialWarning &&
               request.Status == LevyDormancyRequestStatus.Pending &&
               LevyDormancyInactivity.HasBeenInactiveForAtLeast(request, configuration.SwitchMonths, configuration, now);
    }
}
