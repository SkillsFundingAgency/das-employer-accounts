using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.Authentication;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.HomeControllerTests;

public class WhenIRegisterAUser
{
    private Mock<IHomeOrchestrator> _homeOrchestrator;
    private Mock<EmployerAccountsConfiguration> _configuration;
    private HomeController _homeController;
    private Mock<ICookieStorageService<FlashMessageViewModel>> _flashMessage;
    protected Mock<HttpContext> MockHttpContext = new();
    protected ControllerContext ControllerContext;
    private const string Schema = "http";
    private const string Authority = "test.local";
    private const string BaseUrl = "https://baseaddress-hyperlink.com";
    private Mock<IUrlActionHelper> _urlActionHelper;

    [SetUp]
    public void Arrange()
    {
        MockHttpContext.Setup(x => x.Request.Host).Returns(new HostString(Authority));
        MockHttpContext.Setup(x => x.Request.Scheme).Returns(Schema);

        _homeOrchestrator = new Mock<IHomeOrchestrator>();
        _configuration = new Mock<EmployerAccountsConfiguration>();
        _flashMessage = new Mock<ICookieStorageService<FlashMessageViewModel>>();
        _urlActionHelper = new Mock<IUrlActionHelper>();

        _homeController = new HomeController(
            _homeOrchestrator.Object,
            _configuration.Object,
            _flashMessage.Object,
            Mock.Of<ILogger<HomeController>>(), null, null, _urlActionHelper.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = MockHttpContext.Object,
            }
        };
    }

    [TearDown]
    public void TearDown()
    {
        _homeController?.Dispose();
    }

    [Test, AutoData]
    public async Task When_GovSignIn_True_CorrelationId_Is_Null_ThenTheUserIsRedirectedToEmployerProfiles_AddDetailsLink(
        string baseUrl, 
        string redirectUrl)
    {
        //arrange
        _urlActionHelper.Setup(x => x.EmployerProfileAddUserDetails("/user/add-user-details")).Returns(redirectUrl);

        //Act
        var actual = await _homeController.RegisterUser(null);

        //Assert
        Assert.That(actual, Is.Not.Null);
        var actualRedirectResult = actual as RedirectResult;
        Assert.That(actualRedirectResult, Is.Not.Null);
        Assert.That(actualRedirectResult.Url, Is.EqualTo(redirectUrl));
    }

    [Test, MoqAutoData]
    public async Task When_GovSignIn_True_CorrelationId_Is_Given_But_No_Invitation_Data_ThenTheUserIsRedirectedToEmployerProfiles(
        string redirectUrl,
        string baseUrl,
        Guid correlationId)
    {
        //arrange
        _urlActionHelper.Setup(x => x.EmployerProfileAddUserDetails("/user/add-user-details")).Returns(redirectUrl);
        _homeOrchestrator.Setup(x => x.GetProviderInvitation(correlationId)).ReturnsAsync(
            new OrchestratorResponse<ProviderInvitationViewModel>
            {
                Data = null
            });

        //Act
        var actual = await _homeController.RegisterUser(correlationId);

        //Assert
        Assert.That(actual, Is.Not.Null);
        var actualRedirectResult = actual as RedirectResult;
        Assert.That(actualRedirectResult, Is.Not.Null);
        Assert.That(actualRedirectResult.Url, Is.EqualTo(redirectUrl));
    }

    [Test, MoqAutoData]
    public async Task When_GovSignIn_True_ProviderInvitation_Is_Given_ThenTheUserIsRedirectedToEmployerProfiles_AddDetailsLink(
        string redirectUrl,
        string baseUrl,
        Guid correlationId,
        ProviderInvitationViewModel viewModel)
    {
        //arrange
        var queryParams = $"?correlationId={correlationId}&firstname={WebUtility.UrlEncode(viewModel.EmployerFirstName)}&lastname={WebUtility.UrlEncode(viewModel.EmployerLastName)}";
        _homeOrchestrator.Setup(x => x.GetProviderInvitation(correlationId)).ReturnsAsync(
            new OrchestratorResponse<ProviderInvitationViewModel>
            {
                Data = viewModel
            });
        _urlActionHelper.Setup(x =>
                x.EmployerProfileAddUserDetails(
                    $"/user/add-user-details"))
            .Returns(redirectUrl);

        //Act
        var actual = await _homeController.RegisterUser(correlationId);

        //Assert
        Assert.That(actual, Is.Not.Null);
        var actualRedirectResult = actual as RedirectResult;
        Assert.That(actualRedirectResult, Is.Not.Null);
        Assert.That(actualRedirectResult.Url, Is.EqualTo(redirectUrl + queryParams));
    }
}