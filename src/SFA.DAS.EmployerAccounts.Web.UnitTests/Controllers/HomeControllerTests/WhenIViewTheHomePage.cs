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
            Identity = new IdentityServerConfiguration
            {
                BaseAddress = "http://test",
                ChangePasswordLink = "123",
                ChangeEmailLink = "123",
                ClaimIdentifierConfiguration = new ClaimIdentifierConfiguration { ClaimsBaseUrl = "http://claims.test/" }
            },
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

    [Test]
    public async Task ThenTheAccountsAreNotReturnedWhenYouAreNotAuthenticated()
    {
        // Arrange
        AddEmptyUserToContext();

        // Act
        await _homeController.Index(null, null);

        // Asssert
        _homeOrchestrator.Verify(x => x.GetUserAccounts(It.Is<string>(c => c.Equals(string.Empty)),
            null, null, null,
            It.IsAny<DateTime?>()), Times.Never);
    }

    [Test]
    public async Task ThenIfMyAccountIsAuthenticatedButNotActivated()
    {
        // Arrange
        ConfigurationFactory.Current = new IdentityServerConfigurationFactory(
            new EmployerAccountsConfiguration
            {
                Identity = new IdentityServerConfiguration { BaseAddress = "http://test.local/identity", AccountActivationUrl = "/confirm" }
            });
        AddUserToContext(new Claim(ControllerConstants.UserRefClaimKeyName, ExpectedUserId), new Claim(DasClaimTypes.RequiresVerification, "true"));

        // Act
        var actual = await _homeController.Index(null, null);

        // Asssert
        var actualRedirectResult = actual as RedirectResult;
        Assert.IsNotNull(actualRedirectResult);
        Assert.AreEqual("http://test.local/confirm", actualRedirectResult.Url);
    }

    [Test]
    public async Task ThenIfMyAccountIsAuthenticatedButNotActivatedForGovThenIgnoredAndGoesToIndex()
    {
        // Arrange
        _configuration.UseGovSignIn = true;
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
        Assert.IsNotNull(actualRedirectToRouteResult);
        Assert.AreEqual("employer-team-index", actualRedirectToRouteResult.RouteName);
    }
    
    [Test]
    public async Task ThenTheAccountsAreReturnedForThatUserWhenAuthenticated()
    {
        // Arrange
        AddUserToContext(ExpectedUserId, string.Empty, string.Empty,
            new Claim(ControllerConstants.UserRefClaimKeyName, ExpectedUserId),
            new Claim(DasClaimTypes.RequiresVerification, "false")
        );

        _userAccountsViewModel.GaQueryData = _gaQueryData;

        // Act
        await _homeController.Index(_gaQueryData, null);

        // Asssert
        _homeOrchestrator.Verify(x => x.GetUserAccounts(ExpectedUserId,
            _gaQueryData, null, _configuration.ValidRedirectUris,
            It.IsAny<DateTime?>()), Times.Once);
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
                Assert.IsNull(actual);
            }
        }
    }

    [Test]
    public async Task ThenTheUnauthenticatedViewIsReturnedWhenNoUserIsLoggedIn()
    {
        // Arrange
        AddEmptyUserToContext();

        // Act
        var actual = await _homeController.Index(null, null);

        // Asssert
        var actualViewResult = actual as ViewResult;
        Assert.IsNotNull(actualViewResult);
        Assert.AreEqual("ServiceStartPage", actualViewResult.ViewName);
    }

    [Test]
    public async Task ThenTheUnauthenticatedInProd_RedirectedToGovUk_WhenNoUserIsLoggedIn()
    {
        // Arrange
        _mockRootConfig.Setup(x => x["ResourceEnvironmentName"]).Returns("prd");
        _configuration.GovUkSignInToASAccountUrl = "http://gov.uk/account";
        AddEmptyUserToContext();

        // Act
        var actual = await _homeController.Index(null, null);

        // Asssert
        var acutalRedirectResult = actual as RedirectResult;
        Assert.IsNotNull(acutalRedirectResult);
        Assert.AreEqual(_configuration.GovUkSignInToASAccountUrl, acutalRedirectResult.Url);
    }

    [Test]
    public async Task ThenIfIHave_OneAccount_IAmRedirectedToTheEmployerTeamsIndexPage()
    {
        // Arrange
        AddUserToContext(ExpectedUserId, string.Empty, string.Empty,
            new Claim(ControllerConstants.UserRefClaimKeyName, ExpectedUserId),
            new Claim(DasClaimTypes.RequiresVerification, "false")
        );

        // Act
        var actual = await _homeController.Index(_gaQueryData, null);

        // Asssert
        var actualRedirectToRouteResult = actual as RedirectToRouteResult;
        Assert.IsNotNull(actualRedirectToRouteResult);
        Assert.AreEqual(RouteNames.EmployerTeamIndex, actualRedirectToRouteResult.RouteName);
    }

    [Test]
    public async Task ThenIfIHave_OneAccount_IAmRedirectedToTheSpecifiedValidRedirectUri()
    {
        // Arrange
        AddUserToContext(ExpectedUserId, string.Empty, string.Empty,
            new Claim(ControllerConstants.UserRefClaimKeyName, ExpectedUserId),
            new Claim(DasClaimTypes.RequiresVerification, "false")
        );

        var redirectUri = new Uri("https://somewhereovertherainbow/{{hashedAccountId}}/?param=1&otherparam=2");
        var redirectDescription = "land of oz";
        var userAccountsViewModel = SetupUserAccountsViewModel(redirectUri.ToString(), redirectDescription);

        _homeOrchestrator.Setup(x => x.GetUserAccounts(ExpectedUserId, null,
            redirectUri.ToString(), _configuration.ValidRedirectUris,
            It.IsAny<DateTime?>())).ReturnsAsync(
                new OrchestratorResponse<UserAccountsViewModel>
                {
                    Data = userAccountsViewModel
                });

        // Act
        var actual = await _homeController.Index(null, redirectUri.ToString());

        // Assert
        var actualRedirectResult = actual as RedirectResult;
        Assert.IsNotNull(actualRedirectResult);

        var actualRedirectResultUriBuilder = new Uri(actualRedirectResult.Url);
        Assert.AreEqual(
            redirectUri.ReplaceHashedAccountId(userAccountsViewModel.Accounts.AccountList[0].HashedId), 
            actualRedirectResultUriBuilder.ToString());
    }

    [Test]
    public async Task ThenIfIHave_OneAccount_IAmNotRedirectedToTheSpecifiedInvalidRedirectUri()
    {
        // Arrange
        AddUserToContext(ExpectedUserId, string.Empty, string.Empty,
            new Claim(ControllerConstants.UserRefClaimKeyName, ExpectedUserId),
            new Claim(DasClaimTypes.RequiresVerification, "false")
        );

        var redirectUri = new Uri("https://singingintherain/{{hashedAccountId}}/?param=1&otherparam=2");
        var userAccountsViewModel = SetupUserAccountsViewModel(null, null);

        _homeOrchestrator.Setup(x => x.GetUserAccounts(ExpectedUserId, _gaQueryData,
            redirectUri.ToString(), _configuration.ValidRedirectUris,
            It.IsAny<DateTime?>())).ReturnsAsync(
                new OrchestratorResponse<UserAccountsViewModel>
                {
                    Data = userAccountsViewModel
                });

        // Act
        var actual = await _homeController.Index(_gaQueryData, redirectUri.ToString());

        // Assert
        var actualRedirectToRouteResult = actual as RedirectToRouteResult;
        Assert.IsNotNull(actualRedirectToRouteResult);
        Assert.AreEqual(RouteNames.EmployerTeamIndex, actualRedirectToRouteResult.RouteName);
    }

    [Test]
    public async Task ThenIfIHave_OneIncompleteAccount_IAmRedirectedToTheContinueTaskListPage()
    {
        // Arrange
        AddUserToContext(ExpectedUserId, string.Empty, string.Empty,
            new Claim(ControllerConstants.UserRefClaimKeyName, ExpectedUserId),
            new Claim(DasClaimTypes.RequiresVerification, "false")
        );

        _userAccountsViewModel.Accounts.AccountList[0].AddTrainingProviderAcknowledged = false;

        // Act
        var actual = await _homeController.Index(_gaQueryData, null);

        // Asssert
        var actualRedirectToRouteResult = actual as RedirectToRouteResult;
        Assert.IsNotNull(actualRedirectToRouteResult);
        Assert.AreEqual(RouteNames.ContinueNewEmployerAccountTaskList, actualRedirectToRouteResult.RouteName);
    }

    [Test]
    public async Task ThenIfIHaveMoreThanOneAccountIAmRedirectedToTheAccountsIndexPage()
    {
        // Arrange
        AddUserToContext(ExpectedUserId, string.Empty, string.Empty,
            new Claim(ControllerConstants.UserRefClaimKeyName, ExpectedUserId),
            new Claim(DasClaimTypes.RequiresVerification, "false")
        );

        _homeOrchestrator.Setup(x => x.GetUserAccounts(ExpectedUserId,
            null, null, _configuration.ValidRedirectUris,
            It.IsAny<DateTime?>())).ReturnsAsync(
            new OrchestratorResponse<UserAccountsViewModel>
            {
                Data = new UserAccountsViewModel
                {
                    Accounts = new Accounts<Account>
                    {
                        AccountList = new List<Account> { new Account(), new Account() }
                    }
                }
            });

        // Act
        var actual = await _homeController.Index(null, null);

        // Asssert
        var actualViewResult = actual as ViewResult;
        Assert.IsNotNull(actualViewResult);
        Assert.AreEqual(null, actualViewResult.ViewName);
    }

    [Test]
    public async Task ThenIfIHaveMoreThanOneAccountIAmRedirectedToTheAccountsIndexPage_WithTermsAndConditionBannerDisplayed()
    {
        // Arrange
        AddUserToContext(ExpectedUserId, string.Empty, string.Empty,
            new Claim(ControllerConstants.UserRefClaimKeyName, ExpectedUserId),
            new Claim(DasClaimTypes.RequiresVerification, "false")
        );

        _homeOrchestrator.Setup(x => x.GetUserAccounts(ExpectedUserId,
            null, null, _configuration.ValidRedirectUris,
            It.IsAny<DateTime?>())).ReturnsAsync(
            new OrchestratorResponse<UserAccountsViewModel>
            {
                Data = new UserAccountsViewModel
                {
                    Accounts = new Accounts<Account>
                    {
                        AccountList = new List<Account> { new Account(), new Account() }
                    },

                    LastTermsAndConditionsUpdate = DateTime.Now,
                    TermAndConditionsAcceptedOn = DateTime.Now.AddDays(-20)
                }
            });

        // Act
        var actual = await _homeController.Index(null, null);

        // Asssert
        var actualViewResult = actual as ViewResult;
        Assert.IsNotNull(actualViewResult);

        var viewModel = actualViewResult.Model;
        Assert.IsInstanceOf<OrchestratorResponse<UserAccountsViewModel>>(viewModel);
        var userAccountsViewModel = (OrchestratorResponse<UserAccountsViewModel>)viewModel;

        Assert.AreEqual(true, userAccountsViewModel.Data.ShowTermsAndConditionBanner);
    }

    [Test]
    public async Task ThenIfIHaveMoreThanOneAccountIAmRedirectedToTheAccountsIndexPage_WithTermsAndConditionBannerNotDisplayed()
    {
        // Arrange
        AddUserToContext(ExpectedUserId, string.Empty, string.Empty,
            new Claim(ControllerConstants.UserRefClaimKeyName, ExpectedUserId),
            new Claim(DasClaimTypes.RequiresVerification, "false")
        );

        _homeOrchestrator.Setup(x => x.GetUserAccounts(ExpectedUserId,
            null, null, _configuration.ValidRedirectUris,
            It.IsAny<DateTime?>())).ReturnsAsync(
            new OrchestratorResponse<UserAccountsViewModel>
            {
                Data = new UserAccountsViewModel
                {
                    Accounts = new Accounts<Account>
                    {
                        AccountList = new List<Account> { new Account(), new Account() }
                    },

                    LastTermsAndConditionsUpdate = DateTime.Now.AddDays(-20),
                    TermAndConditionsAcceptedOn = DateTime.Now
                }
            });

        // Act
        var actual = await _homeController.Index(null, null);

        // Asssert
        var actualViewResult = actual as ViewResult;
        Assert.IsNotNull(actualViewResult);

        var viewModel = actualViewResult.Model;
        Assert.IsInstanceOf<OrchestratorResponse<UserAccountsViewModel>>(viewModel);
        var userAccountsViewModel = (OrchestratorResponse<UserAccountsViewModel>)viewModel;

        Assert.AreEqual(false, userAccountsViewModel.Data.ShowTermsAndConditionBanner);
    }

    [Test]
    public async Task ThenIfIAmAuthenticatedWithNoProfileInformation()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        _configuration.UseGovSignIn = true;
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
        Assert.AreEqual($"https://employerprofiles.test-eas.apprenticeships.education.gov.uk/user/add-user-details?_ga={_gaQueryData._ga}&_gl={_gaQueryData._gl}&utm_source={_gaQueryData.utm_source}&utm_campaign={_gaQueryData.utm_campaign}&utm_medium={_gaQueryData.utm_medium}&utm_keywords={_gaQueryData.utm_keywords}&utm_content={_gaQueryData.utm_content}", actualViewResult.Url);
    }
    [Test]
    public async Task ThenIfIAmAuthenticatedWithNoUserInformation()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        _configuration.UseGovSignIn = true;
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

        // Asssert
        var actualViewResult = actual as RedirectResult;
        Assert.AreEqual($"https://employerprofiles.test-eas.apprenticeships.education.gov.uk/user/add-user-details?_ga={_gaQueryData._ga}&_gl={_gaQueryData._gl}&utm_source={_gaQueryData.utm_source}&utm_campaign={_gaQueryData.utm_campaign}&utm_medium={_gaQueryData.utm_medium}&utm_keywords={_gaQueryData.utm_keywords}&utm_content={_gaQueryData.utm_content}", actualViewResult.Url);
    }

    [Test]
    public async Task ThenIfIHaveEmployerUserProfile_ButNoEmployerAccountsAndGovSignInTrueIAmRedirectedToTheProfilePage()
    {
        // Arrange
        _configuration.UseGovSignIn = true;
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

        // Asssert
        var actualViewResult = actual as RedirectToRouteResult;
        Assert.AreEqual(RouteNames.NewEmployerAccountTaskList, actualViewResult.RouteName);
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