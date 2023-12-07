using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerAccountControllerTests;

public class EmployerAccountControllerTestsBase
{
    protected static void SetControllerContextUserIdClaim(string userId, EmployerAccountController controller)
    {
        var claims = new List<Claim> { new Claim(ControllerConstants.UserRefClaimKeyName, userId) };
        var claimsIdentity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(claimsIdentity);
        controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
    }
}