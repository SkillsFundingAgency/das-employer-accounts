using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Models.Account;

namespace SFA.DAS.EmployerAccounts.Data.Contracts;

public interface IEmployerAccountRepository
{
    Task<Account> GetAccountById(long accountId);
    Task<Accounts<Account>> GetAccounts(string toDate, int pageNumber, int pageSize);
    Task<Accounts<AccountNameSummary>> GetAccounts(DateTime? since, int pageNumber, int pageSize);

    Task<AccountDetail> GetAccountDetailById(long accountId);
    Task<AccountStats> GetAccountStats(long accountId);
    Task RenameAccount(long accountId, string name);
    Task SetAccountLevyStatus(long accountId, ApprenticeshipEmployerType apprenticeshipEmployerType);
    Task AcknowledgeTrainingProviderTask(long accountId);
}