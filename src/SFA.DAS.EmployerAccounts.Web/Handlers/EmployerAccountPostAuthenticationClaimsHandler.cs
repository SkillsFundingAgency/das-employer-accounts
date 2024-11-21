﻿using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Newtonsoft.Json;
using SFA.DAS.EmployerAccounts.Infrastructure;
using SFA.DAS.EmployerAccounts.Services;
using SFA.DAS.EmployerUsers.WebClientComponents;
using SFA.DAS.GovUK.Auth.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SFA.DAS.EmployerAccounts.Web.Handlers;

public class EmployerAccountPostAuthenticationClaimsHandler(IUserAccountService userAccountService) : ICustomClaims
{
    // To allow unit testing
    public int MaxPermittedNumberOfAccountsOnClaim { get; set; } = SFA.DAS.EmployerAccounts.Constants.MaxNumberOfAssociatedAccountsAllowedOnClaim;

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

        // Some users have 100's of employer accounts. The claims cannot handle that volume of data.
        if (result.EmployerAccounts.Count() <= MaxPermittedNumberOfAccountsOnClaim)
        {
            var accountsAsJson = JsonConvert.SerializeObject(result.EmployerAccounts.ToDictionary(k => k.AccountId));
            var associatedAccountsClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, accountsAsJson, JsonClaimValueTypes.Json);

            claims.Add(associatedAccountsClaim);
        }

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