using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerAccounts.Web.Controllers;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerTeamControllerTests;

public class WhenIChooseIIfIKnowApprenticeshipIsForExistingEmployee
{
    private EmployerTeamController _controller;

    private Mock<ICookieStorageService<FlashMessageViewModel>> _mockCookieStorageService;
    private Mock<EmployerTeamOrchestratorWithCallToAction> _mockEmployerTeamOrchestrator;

    [SetUp]
    public void Arrange()
    {
        _mockCookieStorageService = new Mock<ICookieStorageService<FlashMessageViewModel>>();
        _mockEmployerTeamOrchestrator = new Mock<EmployerTeamOrchestratorWithCallToAction>();

        _controller = new EmployerTeamController(
            _mockCookieStorageService.Object,
            _mockEmployerTeamOrchestrator.Object,
            Mock.Of<IUrlActionHelper>(),
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