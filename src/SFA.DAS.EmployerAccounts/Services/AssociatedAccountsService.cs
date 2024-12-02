using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.GovUK.Auth.Employer;
using EmployerClaims = SFA.DAS.EmployerAccounts.Infrastructure.EmployerClaims;
using EmployerUserAccountItem = SFA.DAS.EmployerAccounts.Models.UserAccounts.EmployerUserAccountItem;

namespace SFA.DAS.EmployerAccounts.Services;

public interface IAssociatedAccountsService
{
    Task<Dictionary<string, GovUK.Auth.Employer.EmployerUserAccountItem>> GetAccounts(bool forceRefresh);
}

public class AssociatedAccountsService(IGovAuthEmployerAccountService accountsService, IHttpContextAccessor httpContextAccessor, ILogger<AssociatedAccountsService> logger) : IAssociatedAccountsService
{
    // To allow unit testing
    public int MaxPermittedNumberOfAccountsOnClaim { get; set; } = Constants.MaxNumberOfAssociatedAccountsAllowedOnClaim;

    /// <summary>
    /// Retrieves a users associated employer accounts from claims.
    /// If the claim is null, the data will be pulled from UserAccountService and persisted to the claims for caching purposes.
    /// </summary>
    /// <param name="forceRefresh">Forces data to be refreshed from UserAccountsService and persisted to user claims regardless of claims state.</param>
    /// <returns>Dictionary of string, EmployerUserAccountItem</returns>
    public async Task<Dictionary<string, GovUK.Auth.Employer.EmployerUserAccountItem>> GetAccounts(bool forceRefresh)
    {
        var user = httpContextAccessor.HttpContext.User;
        var employerAccountsClaim = user.FindFirst(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier));

        if (!forceRefresh && employerAccountsClaim != null)
        {
            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, GovUK.Auth.Employer.EmployerUserAccountItem>>(employerAccountsClaim.Value);
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
    
    private void PersistToClaims(Dictionary<string, GovUK.Auth.Employer.EmployerUserAccountItem> associatedAccounts, Claim employerAccountsClaim, Claim userClaim)
    {
        var accountsAsJson = string.Empty;
        
        // Some users have 100's of employer accounts. The claims cannot handle that volume of data.
        if (associatedAccounts.Count <= MaxPermittedNumberOfAccountsOnClaim)
        {
            accountsAsJson = JsonConvert.SerializeObject(associatedAccounts); 
        }
        
        var associatedAccountsClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, accountsAsJson, JsonClaimValueTypes.Json);

        if (employerAccountsClaim != null)
        {
            userClaim.Subject!.RemoveClaim(employerAccountsClaim);
        }

        userClaim.Subject!.AddClaim(associatedAccountsClaim);
    }
}