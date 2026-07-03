using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerAccounts.Models.LevyDormancy;

namespace SFA.DAS.EmployerAccounts.Commands.LevyDormancy;

public class UpsertEmployerAccountLevyStatusCommandHandler(
    Lazy<EmployerAccountsDbContext> db,
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

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Updated EmployerAccountLevyStatus for account {AccountId}, LastLevyDeclarationDate {LastLevyDeclarationDate}",
            command.AccountId,
            command.LastLevyDeclarationDate);
    }
}
