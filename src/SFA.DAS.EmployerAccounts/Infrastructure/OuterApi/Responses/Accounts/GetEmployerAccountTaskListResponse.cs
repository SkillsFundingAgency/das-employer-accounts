namespace SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Responses.Accounts;

public class GetEmployerAccountTaskListResponse
{
    public IEnumerable<EmployerAccountLegalEntityPermissionItem> EmployerAccountLegalEntityPermissions { get; set; }
}

public class EmployerAccountLegalEntityPermissionItem
{
    public string AccountLegalEntityPublicHashedId { get; set; }
    public string Name { get; set; }
    public string AccountHashedId { get; set; }
}