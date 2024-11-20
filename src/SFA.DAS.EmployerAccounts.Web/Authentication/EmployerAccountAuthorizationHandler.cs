﻿using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using Newtonsoft.Json;
using SFA.DAS.EmployerAccounts.Infrastructure;
using SFA.DAS.EmployerAccounts.Models.UserAccounts;
using SFA.DAS.EmployerAccounts.Web.Authorization;
using SFA.DAS.EmployerAccounts.Web.RouteValues;

namespace SFA.DAS.EmployerAccounts.Web.Authentication;

public interface IEmployerAccountAuthorisationHandler
{
    Task<bool> IsEmployerAuthorised(AuthorizationHandlerContext context, bool allowAllUserRoles);
    Task<bool> IsOutsideAccount(AuthorizationHandlerContext context);
}

public class EmployerAccountAuthorisationHandler(
    IHttpContextAccessor httpContextAccessor,
    ILogger<EmployerAccountAuthorisationHandler> logger,
    IAssociatedAccountsHelper associatedAccountsHelper)
    : IEmployerAccountAuthorisationHandler
{
    public async Task<bool> IsEmployerAuthorised(AuthorizationHandlerContext context, bool allowAllUserRoles)
    {
        if (!httpContextAccessor.HttpContext.Request.RouteValues.ContainsKey(RouteValueKeys.HashedAccountId))
        {
            return false;
        }

        Dictionary<string, EmployerUserAccountItem> employerAccounts;

        try
        {
            employerAccounts = await associatedAccountsHelper.GetAssociatedAccounts(forceRefresh: false);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to retrieve employer account claim for user");
            return false;
        }

        EmployerUserAccountItem employerIdentifier = null;

        var accountIdFromUrl = httpContextAccessor.HttpContext.Request.RouteValues[RouteValueKeys.HashedAccountId].ToString().ToUpper();

        if (employerAccounts != null)
        {
            employerIdentifier = employerAccounts.ContainsKey(accountIdFromUrl)
                ? employerAccounts[accountIdFromUrl]
                : null;
        }

        if (!employerAccounts.ContainsKey(accountIdFromUrl))
        {
            if (!context.User.HasClaim(c => c.Type.Equals(ClaimTypes.NameIdentifier)))
            {
                return false;
            }

            var result = await associatedAccountsHelper.GetAssociatedAccounts(forceRefresh: true);

            var accountsAsJson = JsonConvert.SerializeObject(result);
            var associatedAccountsClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, accountsAsJson, JsonClaimValueTypes.Json);

            var updatedEmployerAccounts = JsonConvert.DeserializeObject<Dictionary<string, EmployerUserAccountItem>>(associatedAccountsClaim.Value);

            if (!updatedEmployerAccounts.ContainsKey(accountIdFromUrl))
            {
                return false;
            }

            employerIdentifier = updatedEmployerAccounts[accountIdFromUrl];
        }

        if (!httpContextAccessor.HttpContext.Items.ContainsKey(ContextItemKeys.EmployerIdentifier))
        {
            httpContextAccessor.HttpContext.Items.Add(ContextItemKeys.EmployerIdentifier, employerAccounts.GetValueOrDefault(accountIdFromUrl));
        }

        return CheckUserRoleForAccess(employerIdentifier, allowAllUserRoles);
    }

    public Task<bool> IsOutsideAccount(AuthorizationHandlerContext context)
    {
        if (httpContextAccessor.HttpContext.Request.RouteValues.ContainsKey(RouteValueKeys.HashedAccountId))
        {
            return Task.FromResult(false);
        }

        if (!context.User.HasClaim(c => c.Type.Equals(ClaimTypes.NameIdentifier)))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    private static bool CheckUserRoleForAccess(EmployerUserAccountItem employerIdentifier, bool allowAllUserRoles)
    {
        if (!Enum.TryParse<EmployerUserRole>(employerIdentifier.Role, true, out var userRole))
        {
            return false;
        }

        return allowAllUserRoles || userRole == EmployerUserRole.Owner;
    }
}