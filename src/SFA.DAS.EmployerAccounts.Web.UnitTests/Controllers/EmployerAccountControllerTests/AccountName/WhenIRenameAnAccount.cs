﻿using FluentAssertions;
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
    private const string AccountNameErrorMessage = "You have entered your organisation name. If you want to use your organisation name select 'Yes, I want to use my organisation name as my employer account name'. If not, enter a new employer account name.";
    private const string AccountNameBlankErrorMessage = "Enter a name";

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

    [TearDown]
    public void TearDown() => _employerAccountController?.Dispose();

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

    [TestCase("", false)] 
    [TestCase(" ", false)] 
    [TestCase("My New Name ~", false)] 
    [TestCase("My New Name {", false)] 
    [TestCase("My New Name }", false)] 
    [TestCase("My New Name |", false)] 
    [TestCase("My New Name ^", false)] 
    [TestCase("My New Name `", false)] 
    [TestCase("<My New Name>", false)]
    [TestCase("My New Name", true)]
    [TestCase("My New Name $@#()\"'!,+-=_:;.&€£*%/[]", true)] 
    public async Task ThenTheNameMustBeValid(string newName, bool isValidInput)
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
        var result = await _employerAccountController.AccountName(hashedAccountId, model);
        var redirectResult = result as RedirectToRouteResult;
        var viewResult = result as ViewResult;

        //Assert
        if (isValidInput)
        {
            viewResult.Should().BeNull();
            redirectResult.Should().NotBeNull();
        }
        else
        {
            viewResult.Should().NotBeNull();
            var viewModel = viewResult!.Model as OrchestratorResponse<RenameEmployerAccountViewModel>;
            viewModel!.Status.Should().Be(HttpStatusCode.BadRequest);
            redirectResult!.Should().BeNull();
        }
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
        model.Data.NewNameError.Should().Be(AccountNameBlankErrorMessage);
    }

    [Test, MoqAutoData]
    public async Task WhenNameIsUnchanged_ThenIShouldReceiveAnError(string hashedAccountId, string accountName)
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
        model.Data.NewNameError.Should().Be(AccountNameErrorMessage);
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


    [Test, MoqAutoData]
    public async Task WhenIUseReservedChars_ThenIMustShouldReceiveAnError(string hashedAccountId)
    {
        //Arrange
        var viewModel = new RenameEmployerAccountViewModel
        {
            ChangeAccountName = true,
            CurrentName = "Test Account",
            NewName = "Test <>"
        };

        //Act
        var result = await _employerAccountController.AccountName(hashedAccountId, viewModel) as ViewResult;
        var model = result.Model.As<OrchestratorResponse<RenameEmployerAccountViewModel>>();

        //Assert
        model.Data.NewNameError.Should().Be("Account name must only include letters a to z, numbers 0 to 9, and special characters such as hyphens, spaces and apostrophes");
    }
}