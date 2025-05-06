using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace SFA.DAS.EmployerAccounts.Queries.SearchEmployerAccountsByName;

public class SearchEmployerAccountsByNameQueryHandler(
    Lazy<EmployerAccountsDbContext> dbContext,
    IValidator<SearchEmployerAccountsByNameQuery> validator)
    : IRequestHandler<SearchEmployerAccountsByNameQuery, List<EmployerAccountByNameResult>>
{
    public async Task<List<EmployerAccountByNameResult>> Handle(SearchEmployerAccountsByNameQuery request, CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(request);

        if (!validationResult.IsValid())
        {
            throw new InvalidRequestException(validationResult.ValidationDictionary);
        }

        if (string.IsNullOrWhiteSpace(request.EmployerName))
        {
            return [];
        }

        var results = await dbContext.Value.AccountLegalEntities
            .Where(ale => ale.Name.StartsWith(request.EmployerName))
            .Include(ale => ale.Account)
            .OrderBy(ale => ale.Name)
            .Select(ale => new EmployerAccountByNameResult
            {
                AccountId = ale.AccountId,
                DasAccountName = ale.Account.Name,
                HashedAccountId = ale.Account.HashedId,
                PublicHashedAccountId = ale.Account.PublicHashedId
            })
            .DistinctBy(ea => ea.AccountId)
            .ToListAsync(cancellationToken);

        return results;
    }
} 