using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Commands.SendNotification;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Models.LevyDormancy;

namespace SFA.DAS.EmployerAccounts.Commands.LevyDormancy;

public class ProcessLevyDormancyWarningsCommandHandler(
    Lazy<EmployerAccountsDbContext> db,
    IMediator mediator,
    IEmployerAccountTeamRepository accountTeamRepository,
    IOptions<LevyDormancyConfiguration> levyDormancyOptions,
    IOptions<EmployerAccountsConfiguration> employerAccountsOptions,
    ICurrentDateTime currentDateTime,
    ILogger<ProcessLevyDormancyWarningsCommandHandler> logger) : IRequestHandler<ProcessLevyDormancyWarningsCommand, ProcessLevyDormancyWarningsResult>
{
    private const string InitialWarningTemplateId = "LevyDormancyInitialWarning";

    public async Task<ProcessLevyDormancyWarningsResult> Handle(
        ProcessLevyDormancyWarningsCommand command,
        CancellationToken cancellationToken)
    {
        var configuration = levyDormancyOptions.Value;
        var result = new ProcessLevyDormancyWarningsResult();

        if (!configuration.OrchestrationEnabled)
        {
            logger.LogInformation("Levy dormancy orchestration is disabled. No warning emails were processed.");
            return result;
        }

        var now = currentDateTime.Now;
        var levyEmployerType = (byte)ApprenticeshipEmployerType.Levy;
        var pendingRequests = await db.Value.LevyDormancyRequests
            .Where(r => (r.Status == LevyDormancyRequestStatus.Pending ||
                         r.Status == LevyDormancyRequestStatus.InProgress) &&
                        r.WarningEmailSentAt == null)
            .ToListAsync(cancellationToken);

        foreach (var request in pendingRequests)
        {
            result.RequestsProcessed++;

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

            if (!IsEligibleForInitialWarning(request, configuration, now))
            {
                result.SkippedNotYetEligible++;

                logger.LogInformation(
                    "Levy dormancy initial warning not yet due for account {AccountId}, request {RequestId}",
                    request.AccountId,
                    request.Id);

                continue;
            }

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

                logger.LogWarning(
                    "No email addresses found to send levy dormancy initial warning for account {AccountId}, request {RequestId}",
                    request.AccountId,
                    request.Id);

                continue;
            }

            var switchDate = now.AddMonths(1);
            var tokens = BuildTokens(account.Name, switchDate, employerAccountsOptions.Value.EmployerAccountsBaseUrl);
            var emailsSentForRequest = 0;

            foreach (var recipient in recipients)
            {
                var recipientTokens = new Dictionary<string, string>(tokens)
                {
                    ["user_first_name"] = recipient.FirstName
                };

                await mediator.Send(new SendNotificationCommand
                {
                    RecipientsAddress = recipient.Email,
                    TemplateId = InitialWarningTemplateId,
                    Tokens = recipientTokens
                }, cancellationToken);

                emailsSentForRequest++;
            }

            request.WarningEmailSentAt = now;
            request.Status = LevyDormancyRequestStatus.InProgress;
            request.UpdatedOn = now;
            result.EmailsSent += emailsSentForRequest;

            logger.LogInformation(
                "Sent levy dormancy initial warning for account {AccountId}, request {RequestId}, recipients {RecipientCount}",
                request.AccountId,
                request.Id,
                emailsSentForRequest);
        }

        if (pendingRequests.Count > 0)
        {
            await db.Value.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation(
            "Levy dormancy warning orchestration completed. Processed {RequestsProcessed}, emails sent {EmailsSent}, cancelled {RequestsCancelled}, skipped no recipients {SkippedNoRecipients}, skipped not yet eligible {SkippedNotYetEligible}",
            result.RequestsProcessed,
            result.EmailsSent,
            result.RequestsCancelled,
            result.SkippedNoRecipients,
            result.SkippedNotYetEligible);

        return result;
    }

    private static bool IsEligibleForInitialWarning(
        LevyDormancyRequest request,
        LevyDormancyConfiguration configuration,
        DateTime now)
    {
        if (request.LastLevyDeclarationDate.HasValue)
        {
            return now >= request.LastLevyDeclarationDate.Value.AddMonths(configuration.InitialWarningMonths);
        }

        var monthsAfterRequestCreation = configuration.InitialWarningMonths - configuration.DormancyDetectionMonths;
        return now >= request.CreatedOn.AddMonths(monthsAfterRequestCreation);
    }

    private static Dictionary<string, string> BuildTokens(string employerName, DateTime switchDate, string employerAccountsBaseUrl)
    {
        return new Dictionary<string, string>
        {
            { "employer_name", employerName },
            { "switch_date", switchDate.ToString("dd MMM yyyy") },
            { "unsubscribe_url", $"{employerAccountsBaseUrl}/settings/notifications" }
        };
    }
}
