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
    private const int InsertBatchSize = 500;

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

        var activeRequestAccountIds = db.Value.LevyDormancyRequests
            .Where(r => r.Status == LevyDormancyRequestStatus.Pending ||
                        r.Status == LevyDormancyRequestStatus.InProgress)
            .Select(r => r.AccountId);

        var ignoredAccountIds = configuration.GetIgnoredAccountIds();

        var query = db.Value.EmployerAccountLevyStatuses
            .AsNoTracking()
            .Join(
                db.Value.Accounts.AsNoTracking(),
                status => status.AccountId,
                account => account.Id,
                (status, account) => new { status, account })
            .Where(x => x.account.ApprenticeshipEmployerType == levyEmployerType
                        && (x.status.LastLevyDeclarationDate == null
                            || x.status.LastLevyDeclarationDate < thresholdDate)
                        && !activeRequestAccountIds.Contains(x.status.AccountId))
            .Select(x => new { x.status.AccountId, x.status.LastLevyDeclarationDate });

        if (ignoredAccountIds.Count > 0)
        {
            var ignoredIds = ignoredAccountIds.ToList();
            query = query.Where(x => !ignoredIds.Contains(x.AccountId));
        }

        var dormantCandidates = await query.ToListAsync(cancellationToken);
        var pendingInserts = 0;

        foreach (var candidate in dormantCandidates)
        {
            result.DormantCandidatesFound++;

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
            pendingInserts++;

            logger.LogInformation(
                "Created LevyDormancyRequest for account {AccountId}. LastLevyDeclarationDate {LastLevyDeclarationDate}, detection threshold {DormancyDetectionMonths} months.",
                candidate.AccountId,
                candidate.LastLevyDeclarationDate,
                dormancyDetectionMonths);

            if (pendingInserts < InsertBatchSize)
            {
                continue;
            }

            await db.Value.SaveChangesAsync(cancellationToken);
            pendingInserts = 0;
        }

        if (pendingInserts > 0)
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
