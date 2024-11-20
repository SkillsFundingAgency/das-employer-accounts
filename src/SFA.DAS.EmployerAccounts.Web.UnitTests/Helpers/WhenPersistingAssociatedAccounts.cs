using System.Security.Claims;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.EmployerAccounts.Infrastructure;
using SFA.DAS.EmployerAccounts.Models.UserAccounts;
using SFA.DAS.EmployerAccounts.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Helpers;

public class WhenPersistingAssociatedAccounts
{
    [Test, MoqAutoData]
    public void Then_User_EmployerAccounts_Should_Be_Populated_ToClaims_When_Within_Maximum_Allowed(
        string userId,
        string email,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        Mock<ILogger<AssociatedAccountsHelper>> logger,
        Mock<IUserAccountService> userAccountService,
        EmployerUserAccounts accountData
    )
    {
        //Arrange
        var claimsPrinciple = new ClaimsPrincipal([
            new ClaimsIdentity([
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.NameIdentifier, userId),
            ])
        ]);

        var httpContext = new DefaultHttpContext(new FeatureCollection())
        {
            User = claimsPrinciple
        };

        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        userAccountService.Setup(x => x.GetUserAccounts(userId, email)).ReturnsAsync(accountData);

        var helper = new AssociatedAccountsHelper(userAccountService.Object, httpContextAccessor.Object, logger.Object)
        {
            MaxPermittedNumberOfAccountsOnClaim = accountData.EmployerAccounts.Count()
        };

        //Act
        helper.PersistToClaims(accountData.EmployerAccounts);

        //Assert
        claimsPrinciple.Claims.Should().Contain(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier));

        var actualClaimValue = claimsPrinciple.Claims.First(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier)).Value;
        var expectedClaimValue = JsonConvert.SerializeObject(accountData.EmployerAccounts.ToDictionary(x => x.AccountId));
        actualClaimValue.Should().Be(expectedClaimValue);
    }

    [Test, MoqAutoData]
    public void Then_User_EmployerAccounts_Should_Not_Be_Populated_ToClaims_When_Exceeds_Maximum_Allowed(
        string userId,
        string email,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        Mock<ILogger<AssociatedAccountsHelper>> logger,
        Mock<IUserAccountService> userAccountService,
        EmployerUserAccounts accountData
    )
    {
        //Arrange
        var claimsPrinciple = new ClaimsPrincipal([
            new ClaimsIdentity([
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.NameIdentifier, userId),
            ])
        ]);

        var httpContext = new DefaultHttpContext(new FeatureCollection())
        {
            User = claimsPrinciple
        };

        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        userAccountService.Setup(x => x.GetUserAccounts(userId, email)).ReturnsAsync(accountData);

        var helper = new AssociatedAccountsHelper(userAccountService.Object, httpContextAccessor.Object, logger.Object)
        {
            MaxPermittedNumberOfAccountsOnClaim = accountData.EmployerAccounts.Count() - 1
        };

        //Act
        helper.PersistToClaims(accountData.EmployerAccounts);

        //Assert
        claimsPrinciple.Claims.Should().NotContain(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier));
    }
}