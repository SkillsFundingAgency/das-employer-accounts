using SFA.DAS.EmployerAccounts.Models.Account;

namespace SFA.DAS.EmployerAccounts.Interfaces;

public interface IEmployerAccountService
{
    Task<EmployerAccountTaskList> GetEmployerAccountTaskList(long accountId, string hashedAccountId);
    Task<TaskSummary> GetTaskSummary(long accountId);
}