
using SFA.DAS.EmployerAccounts.Queries.GetAccountTasks;

namespace SFA.DAS.EmployerAccounts.Queries.GetTaskSummary
{
    public class GetTaskSummaryQuery : IRequest<GetTaskSummaryResponse>
    {
        public long AccountId { get; set; }
    }
}
