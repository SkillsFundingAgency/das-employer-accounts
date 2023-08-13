using SFA.DAS.Common.Domain.Types;

namespace SFA.DAS.EmployerAccounts.Web.Extensions;

public static class AccountDashboardViewModelExtensions
{
    public static bool ShowYourFundingReservationsLink(this AccountDashboardViewModel model)
    {
        return model.ApprenticeshipEmployerType == ApprenticeshipEmployerType.NonLevy;
    }
}