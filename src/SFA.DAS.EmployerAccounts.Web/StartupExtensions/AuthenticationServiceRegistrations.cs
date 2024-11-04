﻿using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.EmployerAccounts.Infrastructure;
using SFA.DAS.EmployerAccounts.Web.Authentication;
using SFA.DAS.EmployerAccounts.Web.Authorization;
using SFA.DAS.EmployerAccounts.Web.Extensions;
using SFA.DAS.GovUK.Auth.Authentication;

namespace SFA.DAS.EmployerAccounts.Web.StartupExtensions;

public static class EmployerAuthenticationServiceRegistrations
{
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddTransient<IEmployerAccountAuthorisationHandler, EmployerAccountAuthorisationHandler>();
        services.AddSingleton<IAuthorizationHandler, EmployerAccountAllRolesAuthorizationHandler>();
        services.AddSingleton<IAuthorizationHandler, EmployerUsersIsOutsideAccountAuthorizationHandler>();
        services.AddSingleton<IAuthorizationHandler, EmployerAccountOwnerAuthorizationHandler>();

        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                PolicyNames.HasUserAccount
                , policy =>
                {
                    policy.RequireClaim(EmployerClaims.IdamsUserIdClaimTypeIdentifier);
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new AccountActiveRequirement());
                });

            options.AddPolicy(
                PolicyNames.HasEmployerOwnerAccount
                , policy =>
                {
                    policy.RequireClaim(EmployerClaims.AccountsClaimsTypeIdentifier);
                    policy.Requirements.Add(new EmployerAccountOwnerRequirement());
                    policy.Requirements.Add(new AccountActiveRequirement());
                    policy.RequireAuthenticatedUser();
                });
            options.AddPolicy(
                PolicyNames.HasEmployerViewerTransactorOwnerAccount
                , policy =>
                {
                    policy.RequireClaim(EmployerClaims.AccountsClaimsTypeIdentifier);
                    policy.Requirements.Add(new EmployerAccountAllRolesRequirement());
                    policy.Requirements.Add(new AccountActiveRequirement());
                    policy.RequireAuthenticatedUser();
                });
            options.AddPolicy(
                PolicyNames.HasEmployerViewerTransactorOwnerAccountOrSupport
                , policy =>
                {
                    policy.RequireAssertion(_ =>
                    {
                        if (_.User.IsSupportUser())
                        {
                            return true;
                        }

                        policy.Requirements.Add(new EmployerAccountAllRolesRequirement());
                        policy.Requirements.Add(new AccountActiveRequirement());

                        return _.User.HasClaim(c => c.Type == EmployerClaims.AccountsClaimsTypeIdentifier);
                    });
                    policy.RequireAuthenticatedUser();
                });
        });

        return services;
    }

    private static bool IsSupportUser(this ClaimsPrincipal user)
    {
        return user.HasClaim(ClaimTypes.Role, SupportUserClaimConstants.Tier1UserClaim) || user.HasClaim(ClaimTypes.Role, SupportUserClaimConstants.Tier2UserClaim);
    }
}
