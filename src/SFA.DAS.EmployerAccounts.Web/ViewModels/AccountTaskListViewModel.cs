using SFA.DAS.EmployerAccounts.Web.RouteValues;

namespace SFA.DAS.EmployerAccounts.Web.ViewModels;

public class AccountTaskListViewModel
{
    public string HashedAccountId { get; set; }
    public bool HasPayeScheme { get; set; }

    public int CompletedSections { get; set; }

    public string SaveProgressRouteName => string.IsNullOrEmpty(HashedAccountId) ? RouteNames.NewAccountSaveProgress : RouteNames.PartialAccountSaveProgress;

    public string AddPayeRouteName => string.IsNullOrEmpty(HashedAccountId) ? RouteNames.EmployerAccountPayBillTriage : RouteNames.AddPayeShutter;

    public bool NameConfirmed { get; internal set; }
    public string EditUserDetailsUrl { get; internal set; }
    public string PendingHashedAgreementId { get; internal set; }
    public bool AgreementAcknowledged { get; set; }
    public bool AddTrainingProviderAcknowledged { get; set; }
    public bool HasSignedAgreement { get; set; }
    public bool HasProviders { get; set; }
    public bool HasProviderPermissions { get; set; }
    public string ProviderPermissionsUrl { get; set; }
    public bool TaskListComplete => HasProviderPermissions || AddTrainingProviderAcknowledged;
}