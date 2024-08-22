using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerAccounts.Commands.RenameEmployerAccount;
using SFA.DAS.EmployerAccounts.Web.RouteValues;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerAccountControllerTests.RenameAccount;

public class WhenIRenameAnAccount : ControllerTestBase
{
    private EmployerAccountController _employerAccountController;
    private Mock<EmployerAccountOrchestrator> _orchestrator;
    private Mock<ICookieStorageService<FlashMessageViewModel>> _flashMessage;
    private Mock<IMediator> _mediator;
    private const string ExpectedRedirectUrl = "http://redirect.local.test";
    private const string AccountNameBlankErrorMessage = "Enter a name";
    private const string AccountNameInvalidErrorMessage = "Account name must only include letters a to z, numbers 0 to 9, and special characters such as hyphens, spaces and apostrophes";

    [SetUp]
    public void Arrange()
    {
        base.Arrange(ExpectedRedirectUrl);

        _orchestrator = new Mock<EmployerAccountOrchestrator>();

        _mediator = new Mock<IMediator>();

        _flashMessage = new Mock<ICookieStorageService<FlashMessageViewModel>>();

        _orchestrator.Setup(x =>
                x.RenameEmployerAccount(It.IsAny<string>(), It.IsAny<RenameEmployerAccountViewModel>(), UserId))
            .ReturnsAsync(new OrchestratorResponse<RenameEmployerAccountViewModel>
            {
                Status = HttpStatusCode.OK,
                Data = new RenameEmployerAccountViewModel()
            });

        AddUserToContext();

        _employerAccountController = new EmployerAccountController(
            _orchestrator.Object,
            Mock.Of<ILogger<EmployerAccountController>>(),
            _flashMessage.Object,
            _mediator.Object,
            Mock.Of<ICookieStorageService<ReturnUrlModel>>(),
            Mock.Of<ICookieStorageService<HashedAccountIdModel>>(),
            Mock.Of<LinkGenerator>())
        {
            ControllerContext = ControllerContext,
            Url = new UrlHelper(new ActionContext(MockHttpContext.Object, Routes, new ActionDescriptor()))
        };
    }

    [TearDown]
    public void TearDown() => _employerAccountController?.Dispose();

    [TestCase("", AccountNameBlankErrorMessage)]
    [TestCase(" ", AccountNameBlankErrorMessage)]
    [TestCase("My New Name ~", AccountNameInvalidErrorMessage)]
    [TestCase("My New Name {", AccountNameInvalidErrorMessage)]
    [TestCase("My New Name }", AccountNameInvalidErrorMessage)]
    [TestCase("My New Name |", AccountNameInvalidErrorMessage)]
    [TestCase("My New Name ^", AccountNameInvalidErrorMessage)]
    [TestCase("My New Name `", AccountNameInvalidErrorMessage)]
    [TestCase("<My New Name>", AccountNameInvalidErrorMessage)]
    public async Task Then_I_Will_Get_An_Error_When_NewName_IsNotValid(string newName, string expectedErrorMessage = "")
    {
        //Arrange
        var model = new RenameEmployerAccountViewModel
        {
            ChangeAccountName = true,
            CurrentName = "Test Account",
            NewName = newName
        };

        var hashedAccountId = Guid.NewGuid().ToString();

        //Act
        var result = await _employerAccountController.RenameAccount(hashedAccountId, model) as ViewResult;

        //Assert
        var modelResult = result.Model as OrchestratorResponse<RenameEmployerAccountViewModel>;
        modelResult.Status.Should().Be(HttpStatusCode.BadRequest);
        modelResult.Data.NewNameError.Should().Be(expectedErrorMessage);
    }

    [TestCase("My New Name")]
    [TestCase("My New Name $@#()\"'!,+-=_:;.&€£*%/[]")]
    public async Task Then_I_Will_Not_Get_An_Error_When_NewName_IsValid(string newName)
    {
        //Arrange
        var model = new RenameEmployerAccountViewModel
        {
            ChangeAccountName = true,
            CurrentName = "Test Account",
            NewName = newName
        };

        var hashedAccountId = Guid.NewGuid().ToString();

        //Act
        var result = await _employerAccountController.RenameAccount(hashedAccountId, model) as RedirectToRouteResult;

        //Assert
        result.RouteName.Should().Be(RouteNames.EmployerTeamIndex);
    }

    [Test, MoqAutoData]
    public async Task Then_Should_Not_Send_CreateAccountCompleteCommand(string hashedAccountId)
    {
        // Arrange
        var viewModel = new RenameEmployerAccountViewModel
        {
            ChangeAccountName = true,
            CurrentName = "Test Account",
            NewName = string.Empty
        };

        _orchestrator
            .Setup(m => m.RenameEmployerAccount(hashedAccountId, viewModel, UserId))
            .ReturnsAsync(new OrchestratorResponse<RenameEmployerAccountViewModel>
            {
                Status = HttpStatusCode.OK
            }).Verifiable();

        // Act
        await _employerAccountController.AccountName(hashedAccountId, viewModel);

        // Assert
        _mediator.Verify(x => x.Send(It.IsAny<RenameEmployerAccountCommand>(), It.IsAny<CancellationToken>()), Times.Never());
    }
}