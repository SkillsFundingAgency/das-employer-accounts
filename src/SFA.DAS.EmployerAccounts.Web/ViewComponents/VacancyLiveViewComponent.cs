namespace SFA.DAS.EmployerAccounts.Web.ViewComponents;

public class VacancyLiveViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(AccountDashboardViewModel model)
    {
        return View(model.CallToActionViewModel.VacanciesViewModel.Vacancy.Status == EmployerAccounts.Models.Recruit.VacancyStatus.Live
            ? model.CallToActionViewModel.VacanciesViewModel.Vacancy : null);
    }
}