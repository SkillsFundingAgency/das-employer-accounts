using AutoFixture;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SFA.DAS.Authentication;
using SFA.DAS.EmployerAccounts.Models.UserProfile;
using SFA.DAS.EmployerAccounts.Web.Extensions;
using SFA.DAS.EmployerAccounts.Web.RouteValues;
using SFA.DAS.GovUK.Auth.Services;
using System.Security.Claims;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.HomeControllerTests;

public class WhenIViewTheHomePage : ControllerTestBase
{
    private UserAccountsViewModel _userAccountsViewModel;
    private HomeController _homeController;
    private Mock<IHomeOrchestrator> _homeOrchestrator;
    private EmployerAccountsConfiguration _configuration;
    private const string ExpectedUserId = "123ABC";
    private UrlActionHelper _urlActionHelper;
    private GaQueryData _gaQueryData;
    private Mock<IConfiguration> _mockRootConfig;

    [SetUp]
    public void Arrange()
    {
        base.Arrange();

        _homeOrchestrator = new Mock<IHomeOrchestrator>();
        _mockRootConfig = new Mock<IConfiguration>();
        var fixture = new Fixture();
        _gaQueryData = fixture.Create<GaQueryData>();

        _configuration = new EmployerAccountsConfiguration
        {
            EmployerPortalBaseUrl = "https://localhost",
            ValidRedirectUris = new List<RedirectUriConfiguration>
            {
                new RedirectUriConfiguration
                {
                    Uri = "https://somewhereovertherainbow/{{hashedAccountId}}/",
                    Description = "land of oz"
                }
            }
        };

        _userAccountsViewModel = SetupUserAccountsViewModel();

        _homeOrchestrator.Setup(x => x.GetUserAccounts(ExpectedUserId,
            null, null, _configuration.ValidRedirectUris, null)).ReturnsAsync(
            new OrchestratorResponse<UserAccountsViewModel>
            {
                Data = _userAccountsViewModel
            });

        _homeOrchestrator.Setup(x => x.GetUserAccounts(ExpectedUserId,
            _gaQueryData, null, _configuration.ValidRedirectUris, null)).ReturnsAsync(
            new OrchestratorResponse<UserAccountsViewModel>
            {
                Data = _userAccountsViewModel
            });

        _mockRootConfig.Setup(x => x["ResourceEnvironmentName"]).Returns("test");
        _urlActionHelper = new UrlActionHelper(_configuration, Mock.Of<IHttpContextAccessor>(), _mockRootConfig.Object);
        _homeController = new HomeController(
            _homeOrchestrator.Object,
            _configuration,
            Mock.Of<ICookieStorageService<FlashMessageViewModel>>(),
            Mock.Of<ILogger<HomeController>>(),
            _mockRootConfig.Object,
            Mock.Of<IStubAuthenticationService>(),
            _urlActionHelper)
        {
            ControllerContext = new ControllerContext { HttpContext = MockHttpContext.Object },
            Url = new UrlHelper(new ActionContext(Mock.Of<HttpContext>(), new RouteData(), new ActionDescriptor()))
        };
    }

    [TearDown]
    public void TearDown()
    {
        _homeController?.Dispose();
    }

    [Test]
    public async Task ThenIfMyAccountIsAuthenticatedButNotActivatedForGovThenIgnoredAndGoesToIndex()
    {
        // Arrange
        AddUserToContext(ExpectedUserId, string.Empty, string.Empty, new Claim(ControllerConstants.UserRefClaimKeyName, ExpectedUserId), new Claim(DasClaimTypes.RequiresVerification, "true"));
        _homeOrchestrator.Setup(x => x.GetUser(ExpectedUserId)).ReturnsAsync(new User
        {
            FirstName = "test",
            LastName = "Tester"
        });
        
        // Act
        var actual = await _homeController.Index(_gaQueryData, null);

        // Asssert
        var actualRedirectToRouteResult = actual as RedirectToRouteResult;
        Assert.That(actualRedirectToRouteResult, Is.Not.Null);
        Assert.That(actualRedirectToRouteResult.RouteName, Is.EqualTo("employer-team-index"));
    }
    
    [Test]
    public void ThenTheIndexDoesNotHaveTheAuthorizeAttribute()
    {
        var methods = typeof(HomeController).GetMethods().Where(m => m.Name.Equals("Index")).ToList();

        foreach (var method in methods)
        {
            var attributes = method.GetCustomAttributes(true).ToList();

            foreach (var attribute in attributes)
            {
                var actual = attribute as AuthorizeAttribute;
                Assert.That(actual, Is.Null);
            }
        }
    }

    [Test]
    public async Task ThenIfIAmAuthenticatedWithNoProfileInformation()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        AddNewGovUserToContext(userId);


        _homeOrchestrator.Setup(x => x.GetUserAccounts(ExpectedUserId,
            null, null, null,
            It.IsAny<DateTime?>())).ReturnsAsync(
            new OrchestratorResponse<UserAccountsViewModel>
            {
                Data = new UserAccountsViewModel
                {
                    Accounts = new Accounts<Account>()
                }
            });
        _homeOrchestrator.Setup(x => x.GetUser(userId)).ReturnsAsync(new User());

        // Act
        var actual = await _homeController.Index(_gaQueryData, null);

        // Asssert
        var actualViewResult = actual as RedirectResult;
        Assert.That(actualViewResult.Url, Is.EqualTo($"https://employerprofiles.test-eas.apprenticeships.education.gov.uk/user/add-user-details?_ga={_gaQueryData._ga}&_gl={_gaQueryData._gl}&utm_source={_gaQueryData.utm_source}&utm_campaign={_gaQueryData.utm_campaign}&utm_medium={_gaQueryData.utm_medium}&utm_keywords={_gaQueryData.utm_keywords}&utm_content={_gaQueryData.utm_content}"));
    }
    [Test]
    public async Task ThenIfIAmAuthenticatedWithNoUserInformation()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        AddNewGovUserToContext(userId);


        _homeOrchestrator.Setup(x => x.GetUserAccounts(ExpectedUserId,
            null, null, null,
            It.IsAny<DateTime?>())).ReturnsAsync(
            new OrchestratorResponse<UserAccountsViewModel>
            {
                Data = new UserAccountsViewModel
                {
                    Accounts = new Accounts<Account>()
                }
            });
        _homeOrchestrator.Setup(x => x.GetUser(userId)).ReturnsAsync((User)null);

        // Act
        var actual = await _homeController.Index(_gaQueryData, null);

        // Assert
        var actualViewResult = actual as RedirectResult;
        Assert.That(actualViewResult.Url, Is.EqualTo($"https://employerprofiles.test-eas.apprenticeships.education.gov.uk/user/add-user-details?_ga={_gaQueryData._ga}&_gl={_gaQueryData._gl}&utm_source={_gaQueryData.utm_source}&utm_campaign={_gaQueryData.utm_campaign}&utm_medium={_gaQueryData.utm_medium}&utm_keywords={_gaQueryData.utm_keywords}&utm_content={_gaQueryData.utm_content}"));
    }

    [Test]
    public async Task ThenIfIHaveEmployerUserProfile_ButNoEmployerAccountsAndGovSignInTrueIAmRedirectedToTheProfilePage()
    {
        // Arrange
        AddNewGovUserToContext(ExpectedUserId);
        _homeOrchestrator.Setup(x => x.GetUser(ExpectedUserId)).ReturnsAsync(new User
        {
            FirstName = "test",
            LastName = "Tester"
        });

        _homeOrchestrator.Setup(x => x.GetUserAccounts(ExpectedUserId,
            _gaQueryData, null, _configuration.ValidRedirectUris,
            It.IsAny<DateTime?>())).ReturnsAsync(
            new OrchestratorResponse<UserAccountsViewModel>
            {
                Data = new UserAccountsViewModel
                {
                    Accounts = new Accounts<Account>
                    {
                        AccountList = new List<Account>()
                    }
                }
            });

        // Act
        var actual = await _homeController.Index(_gaQueryData, null);

        // Assert
        var actualViewResult = actual as RedirectToRouteResult;
        Assert.That(actualViewResult.RouteName, Is.EqualTo(RouteNames.NewEmployerAccountTaskList));
    }

    private static UserAccountsViewModel SetupUserAccountsViewModel(string redirectUri = null, string redirectDescription = null)
    {
        return new UserAccountsViewModel
        {
            RedirectUri = redirectUri,
            RedirectDescription = redirectDescription,
            Accounts = new Accounts<Account>
            {
                AccountList = new List<Account> {
                            new Account
                            {
                                HashedId = "ABC123",
                                NameConfirmed = true,
                                AccountHistory = new List<AccountHistory>
                                {
                                    new AccountHistory()
                                }
                            }
                        }
            }
        };
    }
}