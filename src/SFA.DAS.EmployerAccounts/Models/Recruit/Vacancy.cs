namespace SFA.DAS.EmployerAccounts.Models.Recruit;

public class Vacancy
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public VacancyStatus Status { get; set; }
    public int? NoOfNewApplications { get; set; }
    public int? NoOfSuccessfulApplications { get; set; }
    public int? NoOfUnsuccessfulApplications { get; set; }
    public DateTime? ClosingDate { get; set; }
    public DateTime? ClosedDate { get; set; }
}