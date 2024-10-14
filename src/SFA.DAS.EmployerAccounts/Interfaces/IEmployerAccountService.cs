using SFA.DAS.EmployerAccounts.Models.Account;

namespace SFA.DAS.EmployerAccounts.Interfaces;

public interface IEmployerAccountService
{
    Task<TaskSummary> GetTaskSummary(long accountId);
}