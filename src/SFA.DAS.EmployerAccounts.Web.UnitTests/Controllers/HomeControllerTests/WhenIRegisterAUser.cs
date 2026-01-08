using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

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
    public void When_GovSignIn_True_CorrelationId_Is_Null_ThenTheUserIsRedirectedToEmployerProfiles_AddDetailsLink(
        string baseUrl,
        string redirectUrl)
    {
        //arrange
        _urlActionHelper.Setup(x => x.EmployerProfileAddUserDetails("/user/add-user-details")).Returns(redirectUrl);

        //Act
        var actual = _homeController.RegisterUser();

        //Assert
        actual.Should().NotBeNull();
        var actualRedirectResult = actual as RedirectResult;
        actualRedirectResult.Should().NotBeNull();
        actualRedirectResult.Url.Should().Be(redirectUrl);
    }
}
