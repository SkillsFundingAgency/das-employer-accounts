﻿using Microsoft.Extensions.Logging;
using SFA.DAS.Authentication;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.HomeControllerTests;

public class WhenIModifyMyUserAccount : ControllerTestBase
{
    private Mock<IAuthenticationService> _owinWrapper;
    private Mock<HomeOrchestrator> _homeOrchestrator;
    private EmployerAccountsConfiguration _configuration;      
    private HomeController _homeController;
    private Mock<ICookieStorageService<FlashMessageViewModel>> _flashMessage;

    [SetUp]
    public void Arrange()
    {
        base.Arrange();

        _owinWrapper = new Mock<IAuthenticationService>();
        _homeOrchestrator = new Mock<HomeOrchestrator>();          
        _flashMessage = new Mock<ICookieStorageService<FlashMessageViewModel>>();
        _configuration = new EmployerAccountsConfiguration();

        _homeController = new HomeController(
            _homeOrchestrator.Object,              
            _configuration, 
            _flashMessage.Object,
            Mock.Of<ICookieStorageService<ReturnUrlModel>>(),
            Mock.Of<ILogger<HomeController>>(),
            Mock.Of<IMultiVariantTestingService>())
        {
            ControllerContext = ControllerContext
        };
    }
        
    [Test]
    public void ThenThePasswordChangedActionCreatsARedirectToActionResultToTheIndex()
    {
        //Act
        var actual = _homeController.HandlePasswordChanged();

        //Assert
        Assert.IsNotNull(actual);
        Assert.IsAssignableFrom<RedirectToActionResult>(actual);
        var actualRedirect = actual as RedirectToActionResult;
        Assert.IsNotNull(actualRedirect);
        Assert.AreEqual("Index", actualRedirect.ActionName);
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
    public async Task ThenTheAccountCreatedActionCreatesARedirectToActionResultToIndex()
    {
        //Act
        var actual = await _homeController.HandleNewRegistration();

        //Assert
        Assert.IsNotNull(actual);
        Assert.IsAssignableFrom<RedirectToActionResult>(actual);
        var actualRedirect = actual as RedirectToActionResult;
        Assert.IsNotNull(actualRedirect);
        Assert.AreEqual("Index", actualRedirect.ActionName);
    }

    [Test]
    public async Task ThenTheUserIsUpdatedWhenTheEmailHasChanged()
    {
        //Arrange
        var expectedEmail = "test@test.com";
        var expectedId = "123456";
        var expectedFirstName = "Test";
        var expectedLastName = "tester";
           
        _owinWrapper.Setup(x => x.GetClaimValue("email")).Returns(expectedEmail);
        _owinWrapper.Setup(x => x.GetClaimValue("sub")).Returns(expectedId);
        _owinWrapper.Setup(x => x.GetClaimValue(DasClaimTypes.GivenName)).Returns(expectedFirstName);
        _owinWrapper.Setup(x => x.GetClaimValue(DasClaimTypes.FamilyName)).Returns(expectedLastName);

        //Act
        await _homeController.HandleEmailChanged();

        //Assert
        _owinWrapper.Verify(x => x.UpdateClaims(), Times.Once);
        _homeOrchestrator.Verify(x => x.SaveUpdatedIdentityAttributes(expectedId, expectedEmail, expectedFirstName, expectedLastName, null));
    }
}