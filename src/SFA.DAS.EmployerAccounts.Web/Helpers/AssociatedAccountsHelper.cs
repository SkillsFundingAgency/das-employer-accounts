using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Newtonsoft.Json;
using SFA.DAS.EmployerAccounts.Infrastructure;
using SFA.DAS.EmployerAccounts.Models.UserAccounts;
using SFA.DAS.EmployerAccounts.Services;

namespace SFA.DAS.EmployerAccounts.Web.Helpers;

public interface IAssociatedAccountsHelper
{
    Task<Dictionary<string, EmployerUserAccountItem>> GetAssociatedAccounts(bool forceRefresh);
}

public class AssociatedAccountsHelper(IUserAccountService accountsService, IHttpContextAccessor httpContextAccessor, ILogger<AssociatedAccountsHelper> logger) : IAssociatedAccountsHelper
{
    // To allow unit testing
    public int MaxPermittedNumberOfAccountsOnClaim { get; set; } = WebConstants.MaxNumberOfEmployerAccountsAllowedOnClaim;

    public async Task<Dictionary<string, EmployerUserAccountItem>> GetAssociatedAccounts(bool forceRefresh)
    {
        var user = httpContextAccessor.HttpContext.User;
        var employerAccountsClaim = user.FindFirst(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier));

        if (!forceRefresh && employerAccountsClaim != null)
        {
            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, EmployerUserAccountItem>>(employerAccountsClaim.Value);
            }
            catch (JsonSerializationException e)
            {
                logger.LogError(e, "Could not deserialize employer account claim for user");
                throw;
            }
        }

        var userClaim = user.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier));
        var email = user.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Email))?.Value;
        var userId = userClaim.Value;

        var result = await accountsService.GetUserAccounts(userId, email);
        var associatedAccounts = result.EmployerAccounts.ToDictionary(k => k.AccountId);

        PersistToClaims(associatedAccounts, employerAccountsClaim, userClaim);

        return associatedAccounts;
    }

    private void PersistToClaims(Dictionary<string, EmployerUserAccountItem> associatedAccounts, Claim employerAccountsClaim, Claim userClaim)
    {
        // Some users have 100's of employer accounts. The claims cannot handle that volume of data.
        if (associatedAccounts.Count > MaxPermittedNumberOfAccountsOnClaim)
        {
            return;
        }
        
        var accountsAsJson = JsonConvert.SerializeObject(associatedAccounts);
        var associatedAccountsClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, accountsAsJson, JsonClaimValueTypes.Json);

        if (employerAccountsClaim != null)
        {
            userClaim.Subject!.RemoveClaim(employerAccountsClaim);
        }

        userClaim.Subject!.AddClaim(associatedAccountsClaim);
    }
}