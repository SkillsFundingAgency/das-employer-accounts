namespace SFA.DAS.EmployerAccounts.Queries.SearchEmployerAccountsByName;

public class SearchEmployerAccountsByNameQuery : IRequest<List<EmployerAccountByNameResult>>
{
    public string EmployerName { get; set; }
}

public class EmployerAccountByNameResult
{
    public long AccountId { get; set; }
    public string DasAccountName { get; set; }
    public string HashedAccountId { get; set; }
    public string PublicHashedAccountId { get; set; }
} 