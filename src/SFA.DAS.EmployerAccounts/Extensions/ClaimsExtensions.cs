using System.Security.Claims;

namespace SFA.DAS.EmployerAccounts.Extensions;

public static class ClaimsExtensions
{
    public static bool ClaimsAreEmpty(this ClaimsPrincipal user)
    {
        return !user.Claims.Any();
    }
}