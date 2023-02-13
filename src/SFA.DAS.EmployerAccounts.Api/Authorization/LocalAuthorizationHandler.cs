﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace SFA.DAS.EmployerAccounts.Api.Authorization;

public class LocalAuthorizationHandler : AuthorizationHandler<NoneRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        NoneRequirement requirement)
    {
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}