using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace SFA.DAS.EmployerAccounts.Queries.SearchEmployerAccountsByName;

public class SearchEmployerAccountsByNameQueryHandler(
    Lazy<EmployerAccountsDbContext> dbContext,
    IValidator<SearchEmployerAccountsByNameQuery> validator)
    : IRequestHandler<SearchEmployerAccountsByNameQuery, SearchEmployerAccountsByNameResponse>
{
    public async Task<SearchEmployerAccountsByNameResponse> Handle(SearchEmployerAccountsByNameQuery request, CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(request);

        if (!validationResult.IsValid())
        {
            throw new InvalidRequestException(validationResult.ValidationDictionary);
        }

        var response = new SearchEmployerAccountsByNameResponse();
        
        if (string.IsNullOrWhiteSpace(request.EmployerName))
        {
            return response;
        }

        var results = await dbContext.Value.Accounts
            .Where(account => account.Name.StartsWith(request.EmployerName))
            .OrderBy(account => account.Name)
            .Select(account => new EmployerAccountByNameResult
            {
                AccountId = account.Id,
                DasAccountName = account.Name,
                HashedAccountId = account.HashedId,
                PublicHashedAccountId = account.PublicHashedId
            })
            .ToListAsync(cancellationToken);

        response.EmployerAccounts.AddRange(results);
        
        return response;
    }
} 