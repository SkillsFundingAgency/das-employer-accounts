using System.Threading;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Extensions;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Models.Account;

namespace SFA.DAS.EmployerAccounts.Data;

public class AccountLegalEntityRepository(Lazy<EmployerAccountsDbContext> db) : IAccountLegalEntityRepository
{
    public async Task<List<AccountLegalEntity>> GetAccountLegalEntities(long accountId)
    {
        var accountLegalEntities = await db.Value.AccountLegalEntities
            .Include(x => x.LegalEntity)
            .Include(x => x.Agreements)
            .Where(l =>
                 l.Account.Id == accountId &&
                 (l.PendingAgreementId != null || l.SignedAgreementId != null) &&
                 l.Deleted == null).ToListAsync();

        return accountLegalEntities;
    }

    public async Task<PaginatedList<AccountLegalEntity>> GetAccountLegalEntities(
        string searchTerm,
        List<long> accountIds,
        int pageNumber,
        int pageSize,
        string sortColumn,
        bool isAscending,
        CancellationToken token)
    {
        var query = db.Value.AccountLegalEntities
            .AsNoTracking()
            .Include(x => x.LegalEntity)
            .Include(x => x.Agreements)
            .Where(l =>
                accountIds.Contains(l.Account.Id) &&
                (l.PendingAgreementId != null || l.SignedAgreementId != null) &&
                l.Deleted == null);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(x => x.Name.Contains(searchTerm));
        }

        // Defensive: Ensure sortColumn is not null or empty, fallback to Name
        var safeSortColumn = string.IsNullOrWhiteSpace(sortColumn) ? nameof(AccountLegalEntity.Name) : sortColumn;

        return await query.GetPagedAsync(pageNumber, pageSize, safeSortColumn, isAscending, token);
    }
}