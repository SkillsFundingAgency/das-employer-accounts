namespace SFA.DAS.EmployerAccounts.Web.ViewModels;

public class VacanciesViewModel
{
    public int VacancyCount => Vacancy != null ? 1 : 0;

    public VacancyViewModel Vacancy { get; set; }
}