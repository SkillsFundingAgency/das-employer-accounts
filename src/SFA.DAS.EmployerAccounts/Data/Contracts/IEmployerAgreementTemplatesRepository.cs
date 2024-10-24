using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;

namespace SFA.DAS.EmployerAccounts.Data.Contracts;
public interface IEmployerAgreementTemplatesRepository
{
    public Task<List<EmployerAgreementTemplate>> GetEmployerAgreementTemplates();
}
