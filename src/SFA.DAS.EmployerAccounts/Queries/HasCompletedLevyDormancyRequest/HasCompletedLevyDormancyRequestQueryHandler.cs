using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerAccounts.Models.LevyDormancy;

namespace SFA.DAS.EmployerAccounts.Queries.HasCompletedLevyDormancyRequest;

public class HasCompletedLevyDormancyRequestQueryHandler(Lazy<EmployerAccountsDbContext> db)
    : IRequestHandler<HasCompletedLevyDormancyRequestQuery, HasCompletedLevyDormancyRequestResponse>
{
    public async Task<HasCompletedLevyDormancyRequestResponse> Handle(
        HasCompletedLevyDormancyRequestQuery message,
        CancellationToken cancellationToken)
    {
        var hasCompletedRequest = await db.Value.LevyDormancyRequests
            .AsNoTracking()
            .AnyAsync(
                r => r.AccountId == message.AccountId &&
                     r.Status == LevyDormancyRequestStatus.Completed,
                cancellationToken);

        return new HasCompletedLevyDormancyRequestResponse
        {
            HasCompletedRequest = hasCompletedRequest
        };
    }
}
