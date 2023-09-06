using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerAccounts.Commands.CreateAccountComplete;
using SFA.DAS.EmployerAccounts.Web.RouteValues;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerAccountControllerTests.AccountName;

public class WhenIRenameAnAccount : ControllerTestBase
{
    private EmployerAccountController _employerAccountController;
    private Mock<EmployerAccountOrchestrator> _orchestrator;
    private Mock<ICookieStorageService<FlashMessageViewModel>> _flashMessage;
    private Mock<IMediator> _mediator;
    private const string ExpectedRedirectUrl = "http://redirect.local.test";

    [SetUp]
    public void Arrange()
    {
        base.Arrange(ExpectedRedirectUrl);

        _orchestrator = new Mock<EmployerAccountOrchestrator>();

        _mediator = new Mock<IMediator>();
        
        _flashMessage = new Mock<ICookieStorageService<FlashMessageViewModel>>();

        _orchestrator.Setup(x =>
                x.RenameEmployerAccount(It.IsAny<string>(), It.IsAny<RenameEmployerAccountViewModel>(), It.IsAny<string>()))
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

    [Test, MoqAutoData]
    public async Task ThenIMustConfirmTheRename(string hashedAccountId)
    {
        //Arrange
        var model = new RenameEmployerAccountViewModel
        {
            ChangeAccountName = true,
            CurrentName = "Test Account",
            NewName = "New Account Name"
        };

        //Act
        var result = await _employerAccountController.AccountName(hashedAccountId, model) as RedirectToRouteResult;

        //Assert
        result.RouteName.Should().Be(RouteNames.AccountNameConfirm);
    }

    [Test, MoqAutoData]
    public async Task WhenILeaveNameBlank_ThenIMustShouldRecieveAnError(string hashedAccountId)
    {
        //Arrange
        var viewModel = new RenameEmployerAccountViewModel
        {
            ChangeAccountName = true,
            CurrentName = "Test Account",
            NewName = string.Empty
        };

        //Act
        var result = await _employerAccountController.AccountName(hashedAccountId, viewModel) as ViewResult;
        var model = result.Model.As<OrchestratorResponse<RenameEmployerAccountViewModel>>();

        //Assert
        model.Data.NewNameError.Should().Be("You have not changed your employer account name. Select cancel if you do not want to make any changes or continue if you want to use this");
    }
    
    [Test, MoqAutoData]
    public async Task WhenNameIsUnchanged_ThenIShouldRecieveAnError(string hashedAccountId, string accountName)
    {
        //Arrange
        var viewModel = new RenameEmployerAccountViewModel
        {
            ChangeAccountName = true,
            CurrentName = accountName,
            NewName = accountName
        };

        //Act
        var result = await _employerAccountController.AccountName(hashedAccountId, viewModel) as ViewResult;
        var model = result.Model.As<OrchestratorResponse<RenameEmployerAccountViewModel>>();

        //Assert
        model.Data.NewNameError.Should().Be("You have not changed your employer account name. Select cancel if you do not want to make any changes or continue if you want to use this");
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
            });

        // Act
        await _employerAccountController.AccountName(hashedAccountId, viewModel);

        // Assert
        _mediator.Verify(x => x.Send(It.IsAny<SendAccountTaskListCompleteNotificationCommand>(), It.IsAny<CancellationToken>()), Times.Never());
    }
}