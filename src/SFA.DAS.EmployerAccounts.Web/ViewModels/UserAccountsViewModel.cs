using SFA.DAS.EmployerAccounts.Web.Extensions;

namespace SFA.DAS.EmployerAccounts.Web.ViewModels;

public class UserAccountsViewModel
{
    public Accounts<Account> Accounts { get; set; }
    public int Invitations { get; set; }
    public FlashMessageViewModel FlashMessage { get; set; }
    public string ErrorMessage { get; set; }
    public GaQueryData GaQueryData { get; set; }
    public string RedirectUri { get; set; }
    public string RedirectDescription { get; set; }

    public DateTime? TermAndConditionsAcceptedOn { get; set; }
    public DateTime? LastTermsAndConditionsUpdate { get; set; }

    public string RedirectUriWithHashedAccountId(Account account)
    {
        if (!string.IsNullOrEmpty(RedirectUri))
        {
            var redirectUri = new Uri(RedirectUri);
            return redirectUri.ReplaceHashedAccountId(account.HashedId);
        }

        return null;
    }

    public bool ShowTermsAndConditionBanner
    {
        get
        {
            if (!LastTermsAndConditionsUpdate.HasValue)
            {
                return false;
            }

            return !TermAndConditionsAcceptedOn.HasValue || TermAndConditionsAcceptedOn.Value < LastTermsAndConditionsUpdate.Value;
        }
    }
}