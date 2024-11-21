using System.Security.Claims;
using Newtonsoft.Json;
using SFA.DAS.EmployerAccounts.Infrastructure;

namespace SFA.DAS.EmployerAccounts.Web.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal user)
    {
        return user.FindFirst(EmployerClaims.IdamsUserIdClaimTypeIdentifier)?.Value;
    }
}