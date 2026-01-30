namespace SFA.DAS.EmployerAccounts.Web.ViewComponents;

public class VacancyPendingReviewViewComponent :ViewComponent
{
    public IViewComponentResult Invoke(AccountDashboardViewModel model)
    {
        return View(model.CallToActionViewModel.VacanciesViewModel.Vacancy.Status == EmployerAccounts.Models.Recruit.VacancyStatus.Submitted
            ? model.CallToActionViewModel.VacanciesViewModel.Vacancy : null);
    }
}