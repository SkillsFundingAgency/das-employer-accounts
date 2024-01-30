using SFA.DAS.EmployerAccounts.Interfaces.OuterApi;

namespace SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.Accounts
{
    internal class GetTasksRequest : IGetApiRequest
    {
        private long AccountId { get; }
        public string GetUrl => $"accounts/{AccountId}/teams";

        public GetTasksRequest(long accountId)
        {
            AccountId = accountId;
        }
    }
}