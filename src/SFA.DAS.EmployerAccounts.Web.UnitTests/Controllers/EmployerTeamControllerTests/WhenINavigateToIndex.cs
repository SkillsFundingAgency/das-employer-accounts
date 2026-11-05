using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerAccounts.Web.RouteValues;
using System.Security.Claims;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerTeamControllerTests;

[TestFixture]
internal class WhenINavigateToIndex
{
    private Mock<EmployerTeamOrchestratorWithCallToAction> _orchestratorMock;
    private Mock<IFeature> _featureMock;
    private Mock<ICookieStorageService<FlashMessageViewModel>> _cookieStorageServiceMock;
    private Mock<IUrlActionHelper> _urlActionHelperMock;
    private Mock<ILogger<EmployerTeamController>> _loggerMock;
    private EmployerTeamController _controller;

    private const string HashedAccountId = "ABC123";
    private const string ExternalUserId = "user-guid-123";

    [SetUp]
    public void SetUp()
    {
        _orchestratorMock = new Mock<EmployerTeamOrchestratorWithCallToAction>();
        _featureMock = new Mock<IFeature>();
        _cookieStorageServiceMock = new Mock<ICookieStorageService<FlashMessageViewModel>>();
        _urlActionHelperMock = new Mock<IUrlActionHelper>();
        _loggerMock = new Mock<ILogger<EmployerTeamController>>();


        var claims = new[] { new Claim(ControllerConstants.UserRefClaimKeyName, ExternalUserId) };
        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = claimsPrincipal };

        _controller = new EmployerTeamController(_cookieStorageServiceMock.Object, _orchestratorMock.Object, _urlActionHelperMock.Object, _featureMock.Object, _loggerMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    private static OrchestratorResponse<AccountDashboardViewModel> BuildOkResponse(
        bool? addTrainingProviderAcknowledged = true)
    {
        return new OrchestratorResponse<AccountDashboardViewModel>
        {
            Status = HttpStatusCode.OK,
            Data = new AccountDashboardViewModel
            {
                Account = new Account
                {
                    AddTrainingProviderAcknowledged = addTrainingProviderAcknowledged
                }
            }
        };
    }

    [TestCase(HttpStatusCode.BadRequest)]
    [TestCase(HttpStatusCode.Unauthorized)]
    public async Task Index_WhenResponseStatusIsNotOk_ReturnsViewWithResponse(HttpStatusCode status)
    {
        // Arrange
        var response = new OrchestratorResponse<AccountDashboardViewModel>
        {
            Status = status,
            Data = new AccountDashboardViewModel()
        };

        _orchestratorMock
            .Setup(x => x.GetAccount(HashedAccountId, ExternalUserId))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Index(HashedAccountId);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().Be(response);

        // Feature flag and redirect should NOT have been evaluated
        _featureMock.Verify(x => x.IsFeatureEnabled(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task Index_WhenAddTrainingProviderAcknowledgedIsFalse_RedirectsToTaskListRoute()
    {
        // Arrange
        _orchestratorMock
            .Setup(x => x.GetAccount(HashedAccountId, ExternalUserId))
            .ReturnsAsync(BuildOkResponse(addTrainingProviderAcknowledged: false));

        // Act
        var result = await _controller.Index(HashedAccountId);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToRouteResult>().Subject;
        redirect.RouteName.Should().Be(RouteNames.ContinueNewEmployerAccountTaskList);
        redirect.RouteValues.Should().ContainKey("hashedAccountId")
            .WhoseValue.Should().Be(HashedAccountId);
    }

    [Test]
    public async Task Index_WhenAddTrainingProviderAcknowledgedIsTrue_ReturnsView()
    {
        // Arrange
        _orchestratorMock
            .Setup(x => x.GetAccount(HashedAccountId, ExternalUserId))
            .ReturnsAsync(BuildOkResponse(addTrainingProviderAcknowledged: true));

        _featureMock
            .Setup(x => x.IsFeatureEnabled(It.IsAny<string>()))
            .Returns(false);

        // Act
        var result = await _controller.Index(HashedAccountId);

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [Test]
    public async Task Index_WhenAddTrainingProviderAcknowledgedIsNull_DoesNotRedirect()
    {
        // Arrange
        _orchestratorMock
            .Setup(x => x.GetAccount(HashedAccountId, ExternalUserId))
            .ReturnsAsync(BuildOkResponse(addTrainingProviderAcknowledged: null));

        _featureMock
            .Setup(x => x.IsFeatureEnabled(It.IsAny<string>()))
            .Returns(false);

        // Act
        var result = await _controller.Index(HashedAccountId);

        // Assert
        result.Should().BeOfType<ViewResult>()
            .Which.Model.Should().BeOfType<OrchestratorResponse<AccountDashboardViewModel>>();
    }

    [Test]
    public async Task Index_WhenLevyProjectionTransparencyFeatureEnabled_SetsFlag()
    {
        // Arrange
        _orchestratorMock
            .Setup(x => x.GetAccount(HashedAccountId, ExternalUserId))
            .ReturnsAsync(BuildOkResponse());

        _featureMock
            .Setup(x => x.IsFeatureEnabled(FeatureNames.LevyProjectionTransparency))
            .Returns(true);

        // Act
        var result = await _controller.Index(HashedAccountId);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should()
            .BeOfType<OrchestratorResponse<AccountDashboardViewModel>>().Subject;

        model.Data.LevyProjectionTransparencyEnabled.Should().BeTrue();
    }

    [Test]
    public async Task Index_WhenLevyProjectionTransparencyFeatureDisabled_DoesNotSetFlag()
    {
        // Arrange
        _orchestratorMock
            .Setup(x => x.GetAccount(HashedAccountId, ExternalUserId))
            .ReturnsAsync(BuildOkResponse());

        _featureMock
            .Setup(x => x.IsFeatureEnabled(FeatureNames.LevyProjectionTransparency))
            .Returns(false);

        // Act
        var result = await _controller.Index(HashedAccountId);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should()
            .BeOfType<OrchestratorResponse<AccountDashboardViewModel>>().Subject;

        model.Data.LevyProjectionTransparencyEnabled.Should().BeFalse();
    }

    [Test]
    public async Task Index_WhenNoFlashMessageInCookie_FlashMessageRemainsNull()
    {
        // Arrange
        _orchestratorMock
            .Setup(x => x.GetAccount(HashedAccountId, ExternalUserId))
            .ReturnsAsync(BuildOkResponse());

        _featureMock
            .Setup(x => x.IsFeatureEnabled(It.IsAny<string>()))
            .Returns(false);

        // Act
        var result = await _controller.Index(HashedAccountId);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should()
            .BeOfType<OrchestratorResponse<AccountDashboardViewModel>>().Subject;

        model.FlashMessage.Should().BeNull();
    }

    [Test]
    public void Index_WhenOrchestratorThrows_ExceptionIsPropagated()
    {
        // Arrange
        _orchestratorMock
            .Setup(x => x.GetAccount(HashedAccountId, ExternalUserId))
            .ThrowsAsync(new InvalidOperationException("Orchestrator failure"));

        // Act
        var act = async () => await _controller.Index(HashedAccountId);

        // Assert
        act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Orchestrator failure");
    }

    [Test]
    public async Task Index_AlwaysCallsOrchestratorWithCorrectHashedAccountIdAndUserId()
    {
        // Arrange
        _orchestratorMock
            .Setup(x => x.GetAccount(HashedAccountId, ExternalUserId))
            .ReturnsAsync(BuildOkResponse());

        _featureMock
            .Setup(x => x.IsFeatureEnabled(It.IsAny<string>()))
            .Returns(false);

        // Act
        await _controller.Index(HashedAccountId);

        // Assert
        _orchestratorMock.Verify(
            x => x.GetAccount(HashedAccountId, ExternalUserId),
            Times.Once);
    }
}