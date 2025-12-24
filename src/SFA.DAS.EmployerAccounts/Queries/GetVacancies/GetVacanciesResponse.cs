using SFA.DAS.EmployerAccounts.Models.Recruit;

namespace SFA.DAS.EmployerAccounts.Queries.GetVacancies;

public class GetVacanciesResponse
{
    public Vacancy Vacancy { get; set; }
    public bool HasFailed { get; set; }
}