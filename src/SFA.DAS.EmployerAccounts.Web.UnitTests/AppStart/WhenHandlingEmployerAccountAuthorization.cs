using System.Security.Claims;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.EmployerAccounts.Infrastructure;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Responses.UserAccounts;
using SFA.DAS.EmployerAccounts.Models.UserAccounts;
using SFA.DAS.EmployerAccounts.Services;
using SFA.DAS.EmployerAccounts.Web.Authorization;
using SFA.DAS.EmployerAccounts.Web.RouteValues;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.AppStart;

public class WhenHandlingEmployerAccountAuthorization
{
    [Test, MoqAutoData]
    public async Task Then_Returns_True_If_Employer_Is_Authorized_For_Owner_Role(
        string accountId,
        EmployerAccountOwnerRequirement ownerRequirement,
        EmployerUserAccountItem serviceResponse,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        [Frozen] Mock<IAssociatedAccountsHelper> associatedAccountsHelper,
        EmployerAccountAuthorisationHandler authorizationHandler)
    {
        //Arrange
        serviceResponse.AccountId = accountId.ToUpper();
        serviceResponse.Role = "Owner";
        
        var userAccounts = new EmployerUserAccounts
        {
            EmployerAccounts = new List<EmployerUserAccountItem> { serviceResponse }
        };

        var accountsDictionary = userAccounts.EmployerAccounts.ToDictionary(x => x.AccountId);

        associatedAccountsHelper.Setup(x => x.GetAssociatedAccounts(false))
            .ReturnsAsync(accountsDictionary);
        
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(accountsDictionary));
        var claimsPrinciple = new ClaimsPrincipal([new ClaimsIdentity([claim])]);
        var context = new AuthorizationHandlerContext([ownerRequirement], claimsPrinciple, null);
        var httpContext = new DefaultHttpContext(new FeatureCollection());
        
        httpContext.Request.RouteValues.Add(RouteValueKeys.HashedAccountId, accountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        //Act
        var actual = await authorizationHandler.IsEmployerAuthorised(context, false);

        //Assert
        actual.Should().BeTrue();
    }

    [Test]
    [MoqInlineAutoData("Transactor")]
    [MoqInlineAutoData("Viewer")]
    public async Task Then_Returns_False_If_Employer_Is_Authorized_For_Role_That_Is_Not_Owner(
        string role,
        string accountId,
        EmployerUserAccountItem serviceResponse,
        EmployerAccountOwnerRequirement ownerRequirement,
        [Frozen] Mock<IAssociatedAccountsHelper> associatedAccountsHelper,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorisationHandler authorizationHandler)
    {
        //Arrange
        serviceResponse.Role = role;
        serviceResponse.AccountId = accountId.ToUpper();
        
        var userAccounts = new EmployerUserAccounts
        {
            EmployerAccounts = new List<EmployerUserAccountItem> { serviceResponse }
        };

        var accountsDictionary = userAccounts.EmployerAccounts.ToDictionary(x => x.AccountId);

        associatedAccountsHelper.Setup(x => x.GetAssociatedAccounts(false))
            .ReturnsAsync(accountsDictionary);
        
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(accountsDictionary));
        var claimsPrinciple = new ClaimsPrincipal([new ClaimsIdentity([claim])]);
        var context = new AuthorizationHandlerContext([ownerRequirement], claimsPrinciple, null);
        var httpContext = new DefaultHttpContext(new FeatureCollection());
        
        httpContext.Request.RouteValues.Add(RouteValueKeys.HashedAccountId, accountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        //Act
        var actual = await authorizationHandler.IsEmployerAuthorised(context, false);

        //Assert
        actual.Should().BeFalse();
    }

    [Test]
    [MoqInlineAutoData("Transactor")]
    [MoqInlineAutoData("Viewer")]
    public async Task Then_Viewer_And_Transactor_Are_Allowed_For_All_Roles(
        string role,
        string accountId,
        EmployerAccountOwnerRequirement ownerRequirement,
        EmployerUserAccountItem serviceResponse,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        [Frozen] Mock<IAssociatedAccountsHelper> associatedAccountsHelper,
        EmployerAccountAuthorisationHandler authorizationHandler)
    {
        //Arrange
        serviceResponse.Role = role;
        serviceResponse.AccountId = accountId.ToUpper();
        
        var userAccounts = new EmployerUserAccounts
        {
            EmployerAccounts = new List<EmployerUserAccountItem> { serviceResponse }
        };

        var accountsDictionary = userAccounts.EmployerAccounts.ToDictionary(x => x.AccountId);

        associatedAccountsHelper.Setup(x => x.GetAssociatedAccounts(false))
            .ReturnsAsync(accountsDictionary);

        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(accountsDictionary));
        var claimsPrinciple = new ClaimsPrincipal([new ClaimsIdentity([claim])]);
        var context = new AuthorizationHandlerContext([ownerRequirement], claimsPrinciple, null);
        var httpContext = new DefaultHttpContext(new FeatureCollection());
        
        httpContext.Request.RouteValues.Add(RouteValueKeys.HashedAccountId, accountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        //Act
        var actual = await authorizationHandler.IsEmployerAuthorised(context, true);

        //Assert
        actual.Should().BeTrue();
    }

    [Test, MoqAutoData]
    public async Task Then_Returns_False_If_Employer_Is_Not_Authorized(
        string accountId,
        EmployerIdentifier employerIdentifier,
        EmployerAccountOwnerRequirement ownerRequirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorisationHandler authorizationHandler)
    {
        //Arrange
        employerIdentifier.Role = "Owner";
        employerIdentifier.AccountId = employerIdentifier.AccountId.ToUpper();
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifier } };
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
        var claimsPrinciple = new ClaimsPrincipal([new ClaimsIdentity([claim])]);
        var context = new AuthorizationHandlerContext([ownerRequirement], claimsPrinciple, null);
        var responseMock = new FeatureCollection();
        var httpContext = new DefaultHttpContext(responseMock);
        httpContext.Request.RouteValues.Add(RouteValueKeys.HashedAccountId, accountId.ToUpper());
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        //Act
        var actual = await authorizationHandler.IsEmployerAuthorised(context, false);

        //Assert
        actual.Should().BeFalse();
    }


    [Test, MoqAutoData]
    public async Task Then_If_Not_In_Context_Claims_EmployerAccountService_Checked_And_True_Returned_If_Exists_For_GovSignIn(
        string accountId,
        string userId,
        string email,
        EmployerIdentifier employerIdentifier,
        EmployerAccountOwnerRequirement ownerRequirement,
        EmployerUserAccountItem serviceResponse,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        [Frozen] Mock<IAssociatedAccountsHelper> associatedAccountsHelper,
        [Frozen] Mock<IOptions<EmployerAccountsConfiguration>> configuration,
        EmployerAccountAuthorisationHandler authorizationHandler)
    {
        //Arrange
        serviceResponse.AccountId = accountId.ToUpper();
        serviceResponse.Role = "Owner";

        var userAccounts = new EmployerUserAccounts
        {
            EmployerAccounts = new List<EmployerUserAccountItem> { serviceResponse }
        };

        associatedAccountsHelper.Setup(x => x.GetAssociatedAccounts(false))
            .ReturnsAsync(userAccounts.EmployerAccounts.ToDictionary(x => x.AccountId));

        var userClaim = new Claim(ClaimTypes.NameIdentifier, userId);
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifier } };
        var employerAccountClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
        var claimsPrinciple = new ClaimsPrincipal([new ClaimsIdentity([employerAccountClaim, userClaim, new Claim(ClaimTypes.Email, email)])]);
        var context = new AuthorizationHandlerContext([ownerRequirement], claimsPrinciple, null);
        var responseMock = new FeatureCollection();
        var httpContext = new DefaultHttpContext(responseMock);
        httpContext.Request.RouteValues.Add(RouteValueKeys.HashedAccountId, accountId.ToUpper());
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        //Act
        var actual = await authorizationHandler.IsEmployerAuthorised(context, false);

        //Assert
        actual.Should().BeTrue();
    }

    [Test, MoqAutoData]
    public async Task Then_If_Not_In_Context_Claims_EmployerAccountService_Checked_And_False_Returned_If_Not_Exists(
        string accountId,
        string userId,
        EmployerIdentifier employerIdentifier,
        EmployerAccountOwnerRequirement ownerRequirement,
        EmployerUserAccountItem serviceResponse,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        [Frozen] Mock<IUserAccountService> employerAccountService,
        EmployerAccountAuthorisationHandler authorizationHandler)
    {
        //Arrange
        serviceResponse.AccountId = serviceResponse.AccountId.ToUpper();
        serviceResponse.Role = "Owner";
        employerAccountService.Setup(x => x.GetUserAccounts(userId, ""))
            .ReturnsAsync(new EmployerUserAccounts
            {
                EmployerAccounts = new List<EmployerUserAccountItem> { serviceResponse }
            });

        var userClaim = new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, userId);
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifier } };
        var employerAccountClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
        var claimsPrinciple = new ClaimsPrincipal([new ClaimsIdentity([employerAccountClaim, userClaim])]);
        var context = new AuthorizationHandlerContext([ownerRequirement], claimsPrinciple, null);
        var responseMock = new FeatureCollection();
        var httpContext = new DefaultHttpContext(responseMock);
        httpContext.Request.RouteValues.Add(RouteValueKeys.HashedAccountId, accountId.ToUpper());
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        //Act
        var actual = await authorizationHandler.IsEmployerAuthorised(context, false);

        //Assert
        actual.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public async Task Then_Returns_False_If_Employer_Is_Authorized_But_Has_Invalid_Role_But_Should_Allow_All_known_Roles(
        EmployerIdentifier employerIdentifier,
        EmployerAccountOwnerRequirement ownerRequirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorisationHandler authorizationHandler)
    {
        //Arrange
        employerIdentifier.Role = "Viewer-Owner-Transactor";
        employerIdentifier.AccountId = employerIdentifier.AccountId.ToUpper();
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifier } };
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
        var claimsPrinciple = new ClaimsPrincipal([new ClaimsIdentity([claim])]);
        var context = new AuthorizationHandlerContext([ownerRequirement], claimsPrinciple, null);
        var responseMock = new FeatureCollection();
        var httpContext = new DefaultHttpContext(responseMock);
        httpContext.Request.RouteValues.Add(RouteValueKeys.HashedAccountId, employerIdentifier.AccountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        //Act
        var actual = await authorizationHandler.IsEmployerAuthorised(context, true);

        //Assert
        actual.Should().BeFalse();
    }


    [Test, MoqAutoData]
    public async Task Then_Returns_False_If_AccountId_Not_In_Url(
        EmployerIdentifier employerIdentifier,
        EmployerAccountOwnerRequirement ownerRequirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorisationHandler authorizationHandler)
    {
        //Arrange
        employerIdentifier.Role = "Owner";
        employerIdentifier.AccountId = employerIdentifier.AccountId.ToUpper();
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifier } };
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
        var claimsPrinciple = new ClaimsPrincipal([new ClaimsIdentity([claim])]);
        var context = new AuthorizationHandlerContext([ownerRequirement], claimsPrinciple, null);
        var responseMock = new FeatureCollection();
        var httpContext = new DefaultHttpContext(responseMock);
        httpContext.Request.RouteValues.Clear();
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        //Act
        var actual = await authorizationHandler.IsEmployerAuthorised(context, true);

        //Assert
        actual.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public async Task Then_Returns_False_If_No_Matching_AccountIdentifier_Claim_Found(
        EmployerIdentifier employerIdentifier,
        EmployerAccountOwnerRequirement ownerRequirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorisationHandler authorizationHandler)
    {
        //Arrange
        employerIdentifier.Role = "Viewer-Owner-Transactor";
        employerIdentifier.AccountId = employerIdentifier.AccountId.ToUpper();
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifier } };
        var claim = new Claim("SomeOtherClaim", JsonConvert.SerializeObject(employerAccounts));
        var claimsPrinciple = new ClaimsPrincipal([new ClaimsIdentity([claim])]);
        var context = new AuthorizationHandlerContext([ownerRequirement], claimsPrinciple, null);
        var responseMock = new FeatureCollection();
        var httpContext = new DefaultHttpContext(responseMock);
        httpContext.Request.RouteValues.Add(RouteValueKeys.HashedAccountId, employerIdentifier.AccountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        //Act
        var actual = await authorizationHandler.IsEmployerAuthorised(context, true);

        //Assert
        actual.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public async Task Then_Returns_False_If_No_Matching_NameIdentifier_Claim_Found_For_GovSignIn(
        EmployerIdentifier employerIdentifier,
        EmployerAccountOwnerRequirement ownerRequirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        [Frozen] Mock<IOptions<EmployerAccountsConfiguration>> forecastingConfiguration,
        EmployerAccountAuthorisationHandler authorizationHandler)
    {
        //Arrange
        employerIdentifier.Role = "Viewer-Owner-Transactor";
        employerIdentifier.AccountId = employerIdentifier.AccountId.ToUpper();
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifier } };
        var claim = new Claim("SomeOtherClaim", JsonConvert.SerializeObject(employerAccounts));
        var claimsPrinciple = new ClaimsPrincipal([new ClaimsIdentity([claim])]);
        var context = new AuthorizationHandlerContext([ownerRequirement], claimsPrinciple, null);
        var responseMock = new FeatureCollection();
        var httpContext = new DefaultHttpContext(responseMock);
        httpContext.Request.RouteValues.Add(RouteValueKeys.HashedAccountId, employerIdentifier.AccountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        //Act
        var actual = await authorizationHandler.IsEmployerAuthorised(context, true);

        //Assert
        actual.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public async Task Then_Returns_False_If_The_Claim_Cannot_Be_Deserialized(
        EmployerIdentifier employerIdentifier,
        EmployerAccountOwnerRequirement ownerRequirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorisationHandler authorizationHandler)
    {
        //Arrange
        employerIdentifier.Role = "Owner";
        employerIdentifier.AccountId = employerIdentifier.AccountId.ToUpper();
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerIdentifier));
        var claimsPrinciple = new ClaimsPrincipal([new ClaimsIdentity([claim])]);
        var context = new AuthorizationHandlerContext([ownerRequirement], claimsPrinciple, null);
        var responseMock = new FeatureCollection();
        var httpContext = new DefaultHttpContext(responseMock);
        httpContext.Request.RouteValues.Add(RouteValueKeys.HashedAccountId, employerIdentifier.AccountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        //Act
        var actual = await authorizationHandler.IsEmployerAuthorised(context, true);

        //Assert
        actual.Should().BeFalse();
    }
}