using SFA.DAS.EmployerAccounts.Data.Contracts;

namespace SFA.DAS.EmployerAccounts.Queries.QueryAccounts;

public class QueryAccountsQueryHandler(
    IEmployerAccountRepository employerAccountRepository,
    IValidator<QueryAccountsRequest> validator)
    : IRequestHandler<QueryAccountsRequest, QueryAccountsResponse>
{
    public async Task<QueryAccountsResponse> Handle(QueryAccountsRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid())
        {
            throw new InvalidRequestException(validationResult.ValidationDictionary);
        }

        var includeLegalEntities = request.Include?.Any(i =>
            string.Equals(i, AccountQueryFields.LegalEntities, StringComparison.OrdinalIgnoreCase)) == true;

        var includeEmployerType = request.Select is null || request.Select.Count == 0 || request.Select.Any(s =>
            string.Equals(s, AccountQueryFields.ApprenticeshipEmployerType, StringComparison.OrdinalIgnoreCase));

        var summaries = await employerAccountRepository.GetAccountQuerySummaries(
            request.AccountIds.Distinct().ToList(),
            includeLegalEntities,
            cancellationToken);

        return new QueryAccountsResponse
        {
            Accounts = summaries.Select(summary => new QueryAccountResult
            {
                AccountId = summary.AccountId,
                ApprenticeshipEmployerType = includeEmployerType
                    ? summary.ApprenticeshipEmployerType.ToString()
                    : null,
                LegalEntities = includeLegalEntities
                    ? summary.LegalEntityIds.Select(id => new QueryAccountLegalEntityResult { Id = id.ToString() }).ToList()
                    : []
            }).ToList()
        };
    }
}
