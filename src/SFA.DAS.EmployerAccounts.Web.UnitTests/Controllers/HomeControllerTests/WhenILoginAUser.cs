using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.HomeControllerTests;

public class WhenILoginAUser
{
    private Mock<IHomeOrchestrator> _homeOrchestrator;
    private Mock<EmployerAccountsConfiguration> _configuration;
    private HomeController _homeController;    
    private Mock<ICookieStorageService<FlashMessageViewModel>> _flashMessage;
    private Mock<IUrlActionHelper> _urlActionHelper;

    [SetUp]
    public void Arrange()
    {
        _homeOrchestrator = new Mock<IHomeOrchestrator>();
        _configuration = new Mock<EmployerAccountsConfiguration>();          
        _flashMessage = new Mock<ICookieStorageService<FlashMessageViewModel>>();
        _urlActionHelper = new Mock<IUrlActionHelper>();

        _homeController = new HomeController(
            _homeOrchestrator.Object, 
            _configuration.Object, 
            _flashMessage.Object,
            Mock.Of<ILogger<HomeController>>(), null, null,
            _urlActionHelper.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _homeController?.Dispose();
    }

    [Test]
    public void When_GovSignIn_False_ThenTheUserIsRedirectedToIndex()
    {
        //arrange

        //Act
        var actual = _homeController.SignIn();

        //Assert
        actual.Should().NotBeNull();
        var actualRedirectResult = actual as RedirectToActionResult;
        actualRedirectResult.Should().NotBeNull();
        actualRedirectResult.ActionName.Should().Be(ControllerConstants.IndexActionName);
    }


    [Test]
    public void When_Route_To_PreAuth_ThenTheUserIsRedirectedToIndex()
    {
        //Act
        var actual = _homeController.GovSignIn(null, null);

        //Assert
        actual.Should().NotBeNull();
        var actualRedirectResult = actual as RedirectToActionResult;
        actualRedirectResult.Should().NotBeNull();
        actualRedirectResult.ActionName.Should().Be("Index");
    }
}