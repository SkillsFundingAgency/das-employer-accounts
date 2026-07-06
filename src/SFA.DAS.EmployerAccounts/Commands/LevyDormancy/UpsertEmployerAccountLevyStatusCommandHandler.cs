using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Models.LevyDormancy;

namespace SFA.DAS.EmployerAccounts.Commands.LevyDormancy;

public class UpsertEmployerAccountLevyStatusCommandHandler(
    Lazy<EmployerAccountsDbContext> db,
    IOptions<LevyDormancyConfiguration> levyDormancyOptions,
    ICurrentDateTime currentDateTime,
    ILogger<UpsertEmployerAccountLevyStatusCommandHandler> logger) : IRequestHandler<UpsertEmployerAccountLevyStatusCommand>
{
    public async Task Handle(UpsertEmployerAccountLevyStatusCommand command, CancellationToken cancellationToken)
    {
        var dbContext = db.Value;

        var existing = await dbContext.EmployerAccountLevyStatuses
            .SingleOrDefaultAsync(x => x.AccountId == command.AccountId, cancellationToken);

        if (existing != null && existing.LastRefreshedAt >= command.RefreshedAt)
        {
            logger.LogInformation(
                "Ignoring stale RefreshEmployerLevyDataCompletedEvent for account {AccountId}. Existing LastRefreshedAt {ExistingRefreshedAt}, event {EventRefreshedAt}",
                command.AccountId,
                existing.LastRefreshedAt,
                command.RefreshedAt);

            return;
        }

        if (existing == null)
        {
            dbContext.EmployerAccountLevyStatuses.Add(new EmployerAccountLevyStatus
            {
                AccountId = command.AccountId,
                LastLevyDeclarationDate = command.LastLevyDeclarationDate,
                LastRefreshedAt = command.RefreshedAt
            });
        }
        else
        {
            existing.LastLevyDeclarationDate = command.LastLevyDeclarationDate;
            existing.LastRefreshedAt = command.RefreshedAt;
        }

        var cancelledCount = await CancelActiveDormancyRequestsIfLevyResumed(
            dbContext,
            command.AccountId,
            command.LastLevyDeclarationDate,
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Updated EmployerAccountLevyStatus for account {AccountId}, LastLevyDeclarationDate {LastLevyDeclarationDate}, cancelled dormancy requests {CancelledCount}",
            command.AccountId,
            command.LastLevyDeclarationDate,
            cancelledCount);
    }

    private async Task<int> CancelActiveDormancyRequestsIfLevyResumed(
        EmployerAccountsDbContext dbContext,
        long accountId,
        DateTime? lastLevyDeclarationDate,
        CancellationToken cancellationToken)
    {
        var configuration = levyDormancyOptions.Value;
        var thresholdDate = currentDateTime.Now.AddMonths(-configuration.DormancyDetectionMonths);

        if (!lastLevyDeclarationDate.HasValue || lastLevyDeclarationDate.Value < thresholdDate)
        {
            return 0;
        }

        var activeRequests = await dbContext.LevyDormancyRequests
            .Where(r => r.AccountId == accountId &&
                        (r.Status == LevyDormancyRequestStatus.Pending ||
                         r.Status == LevyDormancyRequestStatus.InProgress))
            .ToListAsync(cancellationToken);

        if (activeRequests.Count == 0)
        {
            return 0;
        }

        var now = currentDateTime.Now;

        foreach (var request in activeRequests)
        {
            request.Status = LevyDormancyRequestStatus.Cancelled;
            request.UpdatedOn = now;

            logger.LogInformation(
                "Cancelled LevyDormancyRequest {RequestId} for account {AccountId} because levy declarations resumed on {LastLevyDeclarationDate}",
                request.Id,
                accountId,
                lastLevyDeclarationDate);
        }

        return activeRequests.Count;
    }
}
