namespace SFA.DAS.EmployerAccounts.Web.ViewComponents;

public class VacancyClosedViewComponent: ViewComponent
{
    public IViewComponentResult Invoke(AccountDashboardViewModel model)
    {
        return View(model.CallToActionViewModel.VacanciesViewModel.Vacancy.Status == EmployerAccounts.Models.Recruit.VacancyStatus.Closed
            ? model.CallToActionViewModel.VacanciesViewModel.Vacancy : null);
    }
}