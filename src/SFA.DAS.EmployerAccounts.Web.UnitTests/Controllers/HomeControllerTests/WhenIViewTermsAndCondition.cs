using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.HomeControllerTests;

public class WhenIViewTermsAndCondition : ControllerTestBase
{
    private Mock<IHomeOrchestrator> _homeOrchestrator;
    private EmployerAccountsConfiguration _configuration;
    private HomeController _homeController;
    private Mock<ICookieStorageService<FlashMessageViewModel>> _flashMessage;
    private Mock<IUrlActionHelper> _urlActionHelper;

    [SetUp]
    public void Arrage()
    {
        base.Arrange();

        AddUserToContext();

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
    public void ThenTheViewIsReturned()
    {
        //Act
        var actual = _homeController.TermsAndConditions("returnUrl", "hashedId");

        //Assert
        Assert.That(actual, Is.Not.Null);
        Assert.IsAssignableFrom<ViewResult>(actual);
    }

    [Test]
    public void ThenTheViewModelIsMappedCorrectly()
    {
        //Act
        var result = _homeController.TermsAndConditions("returnUrl", "hashedId");

        //Assert
        var viewResult = (ViewResult)result;
        var viewModel = viewResult.Model;

        Assert.That(viewModel, Is.InstanceOf<TermsAndConditionsNewViewModel>());
        var termsAndConditionViewModel = (TermsAndConditionsNewViewModel)viewModel;

        Assert.That(termsAndConditionViewModel.ReturnUrl, Is.EqualTo("returnUrl"));
        Assert.That(termsAndConditionViewModel.HashedAccountId, Is.EqualTo("hashedId"));
    }


    [Test]
    public async Task ThenIsRedirectedToEmployerTeamController()
    {
        var termsAndConditionViewModel = new TermsAndConditionsNewViewModel() { HashedAccountId = "HashedId", ReturnUrl = "EmployerTeam" };
        //Act
        var result = await _homeController.TermsAndConditions(termsAndConditionViewModel);

        //Assert
        var redirectResult = (RedirectToActionResult)result;

        Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));
        Assert.That(redirectResult.ControllerName, Is.EqualTo("EmployerTeam"));
    }

    [Test]
    public async Task ThenIsRedirectedToHomeController()
    {
        var termsAndConditionViewModel = new TermsAndConditionsNewViewModel() { HashedAccountId = "HashedId", ReturnUrl = "Home" };
        //Act
        var result = await _homeController.TermsAndConditions(termsAndConditionViewModel);

        //Assert
        var redirectResult = (RedirectToActionResult)result;

        Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));
    }
}