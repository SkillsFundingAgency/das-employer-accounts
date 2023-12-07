using SFA.DAS.EmployerAccounts.Interfaces.OuterApi;

namespace SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.Accounts;

public class GetEmployerAccountTaskListRequest : IGetApiRequest
{
    private long AccountId { get; }
    private string HashedAccountId { get; }
    public string GetUrl => $"accounts/{AccountId}/account-task-list?hashedAccountId={HashedAccountId}";

    public GetEmployerAccountTaskListRequest(long accountId, string hashedAccountId)
    {
        AccountId = accountId;
        HashedAccountId = hashedAccountId;
    }
}