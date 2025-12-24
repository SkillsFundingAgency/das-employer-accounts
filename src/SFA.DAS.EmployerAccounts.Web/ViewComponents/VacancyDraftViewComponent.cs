namespace SFA.DAS.EmployerAccounts.Web.ViewComponents;

public class VacancyDraftViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(AccountDashboardViewModel model)
    {
        return View(model.CallToActionViewModel.VacanciesViewModel.Vacancy.Status == EmployerAccounts.Models.Recruit.VacancyStatus.Draft
            ? model.CallToActionViewModel.VacanciesViewModel.Vacancy : null);
    }
}