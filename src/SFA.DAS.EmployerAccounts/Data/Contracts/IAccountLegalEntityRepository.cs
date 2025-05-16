using System.Threading;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Models.Account;

namespace SFA.DAS.EmployerAccounts.Data.Contracts;

public interface IAccountLegalEntityRepository
{
    Task<List<AccountLegalEntity>> GetAccountLegalEntities(long accountId);
    Task<PaginatedList<AccountLegalEntity>> GetAccountLegalEntities(string searchTerm,
        List<long> accountIds,
        int pageNumber,
        int pageSize,
        string sortColumn,
        bool isAscending,
        CancellationToken token);
}