using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerTeamControllerTests;

public class WhenIChooseIIfIKnowApprenticeshipIsForExistingEmployee
{
    private EmployerTeamController _controller;

    private Mock<ICookieStorageService<FlashMessageViewModel>> _mockCookieStorageService;
    private Mock<EmployerTeamOrchestratorWithCallToAction> _mockEmployerTeamOrchestrator;

    private Mock<IFeature> _mockFeature;

    [SetUp]
    public void Arrange()
    {
        _mockCookieStorageService = new Mock<ICookieStorageService<FlashMessageViewModel>>();
        _mockEmployerTeamOrchestrator = new Mock<EmployerTeamOrchestratorWithCallToAction>();
        _mockFeature = new Mock<IFeature>();

        _controller = new EmployerTeamController(
            _mockCookieStorageService.Object,
            _mockEmployerTeamOrchestrator.Object,
            Mock.Of<IUrlActionHelper>(),
            _mockFeature.Object,
            Mock.Of<ILogger<EmployerTeamController>>()
            );
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    [Test]
    public void IfIChooseYesIContinueTheJourney()
    {
        // Arrange
        var model = new AccountDashboardViewModel
        {
            PayeSchemeCount = 1,
            PendingAgreements = new List<PendingAgreementsViewModel> { new PendingAgreementsViewModel() }
        };

        //Act
        var result = _controller.TriageApprenticeForExistingEmployee(new TriageViewModel { TriageOption = TriageOptions.No }) as ViewResult;

        //Assert
        Assert.That(result.ViewName, Is.EqualTo(ControllerConstants.TriageSetupApprenticeshipNewEmployeeViewName));
    }
}