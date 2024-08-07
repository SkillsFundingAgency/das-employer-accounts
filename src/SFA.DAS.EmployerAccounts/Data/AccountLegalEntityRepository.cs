using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.Account;

namespace SFA.DAS.EmployerAccounts.Data;

public class AccountLegalEntityRepository :  IAccountLegalEntityRepository
{
    private readonly Lazy<EmployerAccountsDbContext> _db;

    public AccountLegalEntityRepository(Lazy<EmployerAccountsDbContext> db)
    {
        _db = db;
    }

    public async Task<List<AccountLegalEntity>> GetAccountLegalEntities(long accountId)
    {
        var accountLegalEntities = await _db.Value.AccountLegalEntities
            .Include(x => x.LegalEntity)
            .Include(x => x.Agreements)
            .Where(l =>
                 l.Account.Id == accountId &&
                 (l.PendingAgreementId != null || l.SignedAgreementId != null) &&
                 l.Deleted == null).ToListAsync();

        return accountLegalEntities;
    }
}