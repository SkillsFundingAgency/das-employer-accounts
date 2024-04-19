using System.Web;

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
            var uriBuilder = new UriBuilder(RedirectUri);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["hashedAccountId"] = account.HashedId;
            uriBuilder.Query = query.ToString();
            return uriBuilder.ToString();
        }

        return null;
    }

    public bool ShowTermsAndConditionBanner { get 
        {
            if (!LastTermsAndConditionsUpdate.HasValue)
            {
                return false;
            }

            return !TermAndConditionsAcceptedOn.HasValue || TermAndConditionsAcceptedOn.Value < LastTermsAndConditionsUpdate.Value;
        } 
    }
}