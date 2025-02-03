using SFA.DAS.EmployerAccounts.Interfaces.OuterApi;

namespace SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.Accounts;

public class GetTasksRequest(long accountId) : IGetApiRequest
{
    private long AccountId { get; } = accountId;
    public string GetUrl => $"accounts/{AccountId}/teams";
}