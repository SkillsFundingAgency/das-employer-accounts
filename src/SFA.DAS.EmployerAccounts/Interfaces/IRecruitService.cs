using SFA.DAS.EmployerAccounts.Models.Recruit;

namespace SFA.DAS.EmployerAccounts.Interfaces;

public interface IRecruitService
{
    Task<Vacancy> GetVacancies(long accountId);
}