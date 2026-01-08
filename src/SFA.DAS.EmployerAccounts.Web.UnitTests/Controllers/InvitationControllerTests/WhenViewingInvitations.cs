using SFA.DAS.EmployerAccounts.Models.AccountTeam;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.InvitationControllerTests;

public class WhenViewingInvitations : ControllerTestBase
{
    private Mock<InvitationOrchestrator> _invitationOrchestrator ;
    private EmployerAccountsConfiguration _configuration ;
    private Mock<ICookieStorageService<FlashMessageViewModel>> _flashMessage;

    private InvitationController _controller;

    [SetUp]
    public void Arrange()
    {
        base.Arrange();

        _invitationOrchestrator = new Mock<InvitationOrchestrator>();
        _configuration = new EmployerAccountsConfiguration();
        _flashMessage = new Mock<ICookieStorageService<FlashMessageViewModel>>();

       _controller = new InvitationController(
            _invitationOrchestrator.Object,
            _configuration,
            _flashMessage.Object)
       {
           ControllerContext = ControllerContext
       };
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    [Test]
    public void ThenTheUserIsShownTheIndexWhenNotAuthenticated()
    {
        //Arrange
        AddEmptyUserToContext();
        
        //Act
        var actual = _controller.Invite();

        //Assert
        actual.Should().NotBeNull();
    }

    [Test]
    public void ThenTheUserIsRedirectedToTheServiceLandingPageWhenAuthenticated()
    {
        //Arrange
        AddUserToContext("my_user_id");
        
        //Act
        var actual = _controller.Invite();

        //Assert
        actual.Should().NotBeNull();
        var actualRedirectResult = actual as RedirectToActionResult;
        actualRedirectResult.Should().NotBeNull();
        actualRedirectResult.ActionName.Should().Be("Index");
        actualRedirectResult.ControllerName.Should().Be("Home");
    }

    [Test]
    public async Task ThenTheCorrectInvitationIsRetrieved()
    {
        //Arrange
        AddUserToContext("TEST");
        _invitationOrchestrator.Setup(x => x.GetInvitation(It.Is<string>(i => i == "123")))
            .ReturnsAsync(new OrchestratorResponse<InvitationView> { Data = new InvitationView() });


        //Act
        var actual = await _controller.Details("123");

        //Assert
        _invitationOrchestrator.Verify(x => x.GetInvitation(It.Is<string>(i => i == "123")));
        actual.Should().NotBeNull();
        var viewResult = actual as ViewResult;
        viewResult.Should().NotBeNull();
    }
}