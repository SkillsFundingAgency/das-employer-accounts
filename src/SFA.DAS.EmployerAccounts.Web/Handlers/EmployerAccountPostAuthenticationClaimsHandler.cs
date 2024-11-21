using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Newtonsoft.Json;
using SFA.DAS.EmployerAccounts.Infrastructure;
using SFA.DAS.EmployerAccounts.Services;
using SFA.DAS.EmployerUsers.WebClientComponents;
using SFA.DAS.GovUK.Auth.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SFA.DAS.EmployerAccounts.Web.Handlers;

public class EmployerAccountPostAuthenticationClaimsHandler(IAssociatedAccountsHelper  associatedAccountsHelper, UserAccountService userAccountService) : ICustomClaims
{
    public int MaxPermittedNumberOfAccountsOnClaim { get; set; } = WebConstants.MaxNumberOfEmployerAccountsAllowedOnClaim;
    
    public async Task<IEnumerable<Claim>> GetClaims(TokenValidatedContext tokenValidatedContext)
    {
        var claims = new List<Claim>();

        var userId = tokenValidatedContext.Principal.Claims
            .First(c => c.Type.Equals(ClaimTypes.NameIdentifier))
            .Value;
        
        var email = tokenValidatedContext.Principal.Claims
            .First(c => c.Type.Equals(ClaimTypes.Email))
            .Value;
        
        claims.Add(new Claim(EmployerClaims.IdamsUserEmailClaimTypeIdentifier, email));
        
        var result = await userAccountService.GetUserAccounts(userId, email);
        associatedAccountsHelper.PersistToClaims(result.EmployerAccounts.ToList());
        
        if (result.IsSuspended)
        {
            claims.Add(new Claim(ClaimTypes.AuthorizationDecision, "Suspended"));
        }
        
        claims.Add(new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, result.EmployerUserId));
        
        if (!string.IsNullOrEmpty(result.FirstName) && !string.IsNullOrEmpty(result.LastName))
        {
            claims.Add(new Claim(DasClaimTypes.GivenName, result.FirstName));
            claims.Add(new Claim(DasClaimTypes.FamilyName, result.LastName));
            claims.Add(new Claim(EmployerClaims.IdamsUserDisplayNameClaimTypeIdentifier, result.FirstName + " " + result.LastName));    
        }
        
        return claims;
    }
}