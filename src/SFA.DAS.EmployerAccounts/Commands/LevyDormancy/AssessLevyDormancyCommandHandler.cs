using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Models.LevyDormancy;

namespace SFA.DAS.EmployerAccounts.Commands.LevyDormancy;

public class AssessLevyDormancyCommandHandler(
    Lazy<EmployerAccountsDbContext> db,
    IOptions<LevyDormancyConfiguration> levyDormancyOptions,
    ICurrentDateTime currentDateTime,
    ILogger<AssessLevyDormancyCommandHandler> logger) : IRequestHandler<AssessLevyDormancyCommand, AssessLevyDormancyResult>
{
    public async Task<AssessLevyDormancyResult> Handle(AssessLevyDormancyCommand command, CancellationToken cancellationToken)
    {
        var configuration = levyDormancyOptions.Value;
        var result = new AssessLevyDormancyResult();

        if (!configuration.AssessmentEnabled)
        {
            logger.LogInformation("Levy dormancy assessment is disabled. No accounts were assessed.");
            return result;
        }

        var assessedOn = currentDateTime.Now;
        var dormancyDetectionMonths = configuration.DormancyDetectionMonths;
        var thresholdDate = assessedOn.AddMonths(-dormancyDetectionMonths);
        var levyEmployerType = (byte)ApprenticeshipEmployerType.Levy;

        result.AccountsAssessed = await db.Value.Accounts
            .CountAsync(a => a.ApprenticeshipEmployerType == levyEmployerType, cancellationToken);

        if (result.AccountsAssessed == 0)
        {
            logger.LogInformation("Levy dormancy assessment completed. No levy accounts found.");
            return result;
        }

        var accountIdsWithActiveRequests = await db.Value.LevyDormancyRequests
            .Where(r => r.Status == LevyDormancyRequestStatus.Pending ||
                        r.Status == LevyDormancyRequestStatus.InProgress)
            .Select(r => r.AccountId)
            .ToListAsync(cancellationToken);

        var ignoredAccountIds = configuration.GetIgnoredAccountIds();
        var dormantCandidates = await db.Value.EmployerAccountLevyStatuses
            .Where(status => db.Value.Accounts.Any(account =>
                account.Id == status.AccountId &&
                account.ApprenticeshipEmployerType == levyEmployerType))
            .Where(status =>
                status.LastLevyDeclarationDate == null ||
                status.LastLevyDeclarationDate < thresholdDate)
            .Where(status => !accountIdsWithActiveRequests.Contains(status.AccountId))
            .ToListAsync(cancellationToken);

        foreach (var candidate in dormantCandidates)
        {
            result.DormantCandidatesFound++;

            if (ignoredAccountIds.Contains(candidate.AccountId))
            {
                result.DormancyRequestsSkippedIgnored++;

                logger.LogInformation(
                    "Skipping LevyDormancyRequest for ignored account {AccountId}",
                    candidate.AccountId);

                continue;
            }

            db.Value.LevyDormancyRequests.Add(new LevyDormancyRequest
            {
                AccountId = candidate.AccountId,
                NoLevyDeclaredMonths = dormancyDetectionMonths,
                LastLevyDeclarationDate = candidate.LastLevyDeclarationDate,
                Status = LevyDormancyRequestStatus.Pending,
                CreatedOn = assessedOn,
                UpdatedOn = assessedOn
            });

            result.DormancyRequestsCreated++;

            logger.LogInformation(
                "Created LevyDormancyRequest for account {AccountId}. LastLevyDeclarationDate {LastLevyDeclarationDate}, detection threshold {DormancyDetectionMonths} months.",
                candidate.AccountId,
                candidate.LastLevyDeclarationDate,
                dormancyDetectionMonths);
        }

        if (result.DormancyRequestsCreated > 0)
        {
            await db.Value.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation(
            "Levy dormancy assessment completed. Assessed {AccountsAssessed}, dormant candidates {DormantCandidatesFound}, dormancy requests created {DormancyRequestsCreated}, skipped ignored {DormancyRequestsSkippedIgnored}",
            result.AccountsAssessed,
            result.DormantCandidatesFound,
            result.DormancyRequestsCreated,
            result.DormancyRequestsSkippedIgnored);

        return result;
    }
}
