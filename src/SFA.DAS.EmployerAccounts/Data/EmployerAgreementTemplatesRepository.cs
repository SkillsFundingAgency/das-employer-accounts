using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.Account;

namespace SFA.DAS.EmployerAccounts.Data;

[ExcludeFromCodeCoverage]
public class EmployerAgreementTemplatesRepository : IEmployerAgreementTemplatesRepository
{
    private readonly Lazy<EmployerAccountsDbContext> _db;

    public EmployerAgreementTemplatesRepository(Lazy<EmployerAccountsDbContext> db)
    {
        _db = db;
    }

    public async Task<List<AgreementTemplate>> GetEmployerAgreementTemplates()
    {
        return await _db.Value.AgreementTemplates.ToListAsync();
    }
}
