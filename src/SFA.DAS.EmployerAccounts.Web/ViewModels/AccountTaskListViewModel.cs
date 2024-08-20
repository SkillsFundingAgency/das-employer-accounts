using SFA.DAS.EmployerAccounts.Web.RouteValues;

namespace SFA.DAS.EmployerAccounts.Web.ViewModels;

public class AccountTaskListViewModel
{
    public string HashedAccountId { get; set; }
    public bool HasPayeScheme { get; set; }

    public int CompletedSections =>
        // by default, will have 1 completed section for user details (step previous)
        AgreementAcknowledged ? 4 : NameConfirmed ? 3 : HasPayeScheme ? 2 : 1;

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