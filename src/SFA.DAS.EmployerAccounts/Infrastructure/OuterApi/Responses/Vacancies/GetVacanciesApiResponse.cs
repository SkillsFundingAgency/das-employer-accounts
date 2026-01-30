using Newtonsoft.Json;

namespace SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Responses.Vacancies;

public class GetVacanciesApiResponse
{
    public IEnumerable<VacancySummary> Vacancies { get; set; }
}

public class VacancySummary
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Status { get; set; }
    public int? NoOfNewApplications { get; set; }
    public int? NoOfSuccessfulApplications { get; set; }
    public int? NoOfUnsuccessfulApplications { get; set; }
    public DateTime? ClosingDate { get; set; }
    public DateTime? ClosedDate { get; set; }
}