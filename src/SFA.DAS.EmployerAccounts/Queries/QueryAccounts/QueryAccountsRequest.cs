namespace SFA.DAS.EmployerAccounts.Queries.QueryAccounts;

public class QueryAccountsRequest : IRequest<QueryAccountsResponse>
{
    public const int MaxAccountIds = 100;

    public List<long> AccountIds { get; set; } = [];
    public List<string> Select { get; set; } = [];
    public List<string> Include { get; set; } = [];
}

public class QueryAccountsResponse
{
    public List<QueryAccountResult> Accounts { get; set; } = [];
}

public class QueryAccountResult
{
    public long AccountId { get; set; }
    public string ApprenticeshipEmployerType { get; set; }
    public List<QueryAccountLegalEntityResult> LegalEntities { get; set; } = [];
}

public class QueryAccountLegalEntityResult
{
    public string Id { get; set; }
}

public static class AccountQueryFields
{
    public const string ApprenticeshipEmployerType = "apprenticeshipEmployerType";
    public const string LegalEntities = "legalEntities";
}
