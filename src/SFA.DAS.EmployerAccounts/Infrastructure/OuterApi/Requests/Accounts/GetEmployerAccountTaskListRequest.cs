using SFA.DAS.EmployerAccounts.Interfaces.OuterApi;

namespace SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.Accounts;

public class GetEmployerAccountTaskListRequest : IGetApiRequest
{
    public string HashedAccountId { get; }
    public string GetUrl => $"accounts/{HashedAccountId}/account-task-list";

    public GetEmployerAccountTaskListRequest(string hashedAccountId)
    {
        HashedAccountId = hashedAccountId;
    }
}