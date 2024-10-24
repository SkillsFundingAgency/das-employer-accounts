using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;

namespace SFA.DAS.EmployerAccounts.Data;
public class EmployerAgreementTemplatesRepository : IEmployerAgreementTemplatesRepository
{
    private readonly Lazy<EmployerAccountsDbContext> _db;

    public EmployerAgreementTemplatesRepository(Lazy<EmployerAccountsDbContext> db)
    {
        _db = db;
    }

    public async Task<List<EmployerAgreementTemplate>> GetEmployerAgreementTemplates()
    {
        var result = await _db.Value.Database.GetDbConnection().QueryAsync<EmployerAgreementTemplate>(
            sql: "select * from [Employer_account].[EmployerAgreementTemplate]",
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.Text);

        return result.ToList();
    }
}
