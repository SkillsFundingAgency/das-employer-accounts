using Microsoft.Extensions.Logging;
using SFA.DAS.Authentication;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.HomeControllerTests;

public class WhenIModifyMyUserAccount : ControllerTestBase
{
    private Mock<IAuthenticationService> _owinWrapper;
    private Mock<IHomeOrchestrator> _homeOrchestrator;
    private EmployerAccountsConfiguration _configuration;
    private HomeController _homeController;
    private Mock<ICookieStorageService<FlashMessageViewModel>> _flashMessage;
    private Mock<IUrlActionHelper> _urlActionHelper;

    [SetUp]
    public void Arrange()
    {
        base.Arrange();

        _owinWrapper = new Mock<IAuthenticationService>();
        _homeOrchestrator = new Mock<IHomeOrchestrator>();
        _flashMessage = new Mock<ICookieStorageService<FlashMessageViewModel>>();
        _configuration = new EmployerAccountsConfiguration();
        _urlActionHelper = new Mock<IUrlActionHelper>();

        _homeController = new HomeController(
            _homeOrchestrator.Object,
            _configuration,
            _flashMessage.Object,
            Mock.Of<ILogger<HomeController>>(), null, null,
            _urlActionHelper.Object)
        {
            ControllerContext = ControllerContext
        };
    }

    [TearDown]
    public void TearDown()
    {
        _homeController?.Dispose();
    }

    [Test]
    public void ThenThePasswordChangedActionCreatsARedirectToActionResultToTheIndex()
    {
        //Act
        var actual = _homeController.HandlePasswordChanged();

        //Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual, Is.AssignableFrom<RedirectToActionResult>());
        var actualRedirect = actual as RedirectToActionResult;
        Assert.That(actualRedirect, Is.Not.Null);
        Assert.That(actualRedirect.ActionName, Is.EqualTo("Index"));
    }

    [Test]
    public async Task ThenIfTheHandleEmailChangedIsCancelledAndTheQueryParamIsSetTheCookieValuesAreNotSet()
    {
        //Act
        await _homeController.HandleEmailChanged(true);

        //Assert
        _flashMessage.Verify(x => x.Create(It.Is<FlashMessageViewModel>(c => c.Headline.Equals("You've changed your email")), It.IsAny<string>(), 1), Times.Never);
        _owinWrapper.Verify(x => x.UpdateClaims(), Times.Never);
    }


    [Test]
    public void ThenIfTheHandlePasswordChangedIsCancelledAndTheQueryParamIsSetTheCookieValuesAreNotSet()
    {
        //Act
        _homeController.HandlePasswordChanged(true);

        //Assert
        _flashMessage.Verify(x => x.Create(It.Is<FlashMessageViewModel>(c => c.Headline.Equals("You've changed your password")), It.IsAny<string>(), 1), Times.Never);
    }

    [Test]
    public void ThenTheAccountCreatedActionCreatesARedirectToActionResultToIndex()
    {
        //Act
        var actual = _homeController.HandleNewRegistration();

        //Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual, Is.AssignableFrom<RedirectToActionResult>());
        var actualRedirect = actual as RedirectToActionResult;
        Assert.That(actualRedirect, Is.Not.Null);
        Assert.That(actualRedirect.ActionName, Is.EqualTo("Index"));
    }

    [Test]
    public void ThenTheAccountCreatedActionCreatesARedirectToActionResultToIndexAndDoesntUpdateDetailsForGovSignIn()
    {
        //Arrange
        _homeController = new HomeController(
            _homeOrchestrator.Object,
            _configuration,
            _flashMessage.Object,
            Mock.Of<ILogger<HomeController>>(), null, null,
            _urlActionHelper.Object)
        {
            ControllerContext = ControllerContext
        };

        //Act
        var actual = _homeController.HandleNewRegistration("123-345");

        //Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual, Is.AssignableFrom<RedirectToActionResult>());
        var actualRedirect = actual as RedirectToActionResult;
        Assert.That(actualRedirect, Is.Not.Null);
        Assert.That(actualRedirect.ActionName, Is.EqualTo("Index"));
        _homeOrchestrator.Verify(x => x.SaveUpdatedIdentityAttributes(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task ThenTheUserIsUpdatedWhenTheEmailHasChanged()
    {
        //Arrange
        const string expectedEmail = "test@test.com";
        const string expectedId = "123456";
        const string expectedFirstName = "Test";
        const string expectedLastName = "tester";

        AddUserToContext(expectedId, expectedEmail, expectedFirstName, expectedLastName);

        //Act
        await _homeController.HandleEmailChanged();

        //Assert
        _homeOrchestrator.Verify(x => x.SaveUpdatedIdentityAttributes(expectedId, expectedEmail, expectedFirstName, expectedLastName, null));
    }
}