using SFA.DAS.EmployerAccounts.Interfaces.OuterApi;

namespace SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.Accounts;

public record GetCreateTaskListRequest(long AccountId, string HashedAccountId, string UserRef) : IGetApiRequest
{
    public string GetUrl => $"accounts/{AccountId}/create-task-list?hashedAccountId={HashedAccountId}&userRef={UserRef}";
}