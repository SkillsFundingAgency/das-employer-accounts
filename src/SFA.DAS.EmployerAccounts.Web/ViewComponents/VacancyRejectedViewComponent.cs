namespace SFA.DAS.EmployerAccounts.Web.ViewComponents;

public class VacancyRejectedViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(AccountDashboardViewModel model)
    {
        return View(model.CallToActionViewModel.VacanciesViewModel.Vacancy.Status == EmployerAccounts.Models.Recruit.VacancyStatus.Referred
            ? model.CallToActionViewModel.VacanciesViewModel.Vacancy : null);
    }
}