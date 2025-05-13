namespace SFA.DAS.EmployerAccounts.Queries.SearchEmployerAccountsByName;

public class SearchEmployerAccountsByNameQuery : IRequest<SearchEmployerAccountsByNameResponse>
{
    public string EmployerName { get; set; }
}

public class SearchEmployerAccountsByNameResponse
{
    public List<EmployerAccountByNameResult> EmployerAccounts { get; init; } = [];
}

public class EmployerAccountByNameResult
{
    public long AccountId { get; set; }
    public string DasAccountName { get; set; }
    public string HashedAccountId { get; set; }
    public string PublicHashedAccountId { get; set; }
} 