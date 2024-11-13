using SFA.DAS.EmployerAccounts.Models.Account;

namespace SFA.DAS.EmployerAccounts.Data.Contracts;

public interface IEmployerAgreementTemplatesRepository
{
    public Task<List<AgreementTemplate>> GetEmployerAgreementTemplates();
}
