using Microsoft.AspNetCore.Authorization;
using SFA.DAS.EmployerAccounts.Web.Authentication;

namespace SFA.DAS.EmployerAccounts.Web.Authorization;

public class EmployerAccountOwnerAuthorizationHandler(IEmployerAccountAuthorisationHandler handler) : AuthorizationHandler<EmployerAccountOwnerRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, EmployerAccountOwnerRequirement ownerRequirement)
    {
        if (!await handler.IsEmployerAuthorised(context, false))
        {
            return;
        }

        context.Succeed(ownerRequirement);
    }
}

