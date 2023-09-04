using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Dtos;
using SFA.DAS.EmployerAccounts.Infrastructure;
using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;
using SFA.DAS.EmployerAccounts.Queries.GetAccountLegalEntitiesCountByHashedAccountId;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAgreement;
using SFA.DAS.EmployerAccounts.Queries.GetLastSignedAgreement;
using SFA.DAS.EmployerAccounts.Web.RouteValues;
using SFA.DAS.Testing;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers;

[TestFixture]
public class EmployerAgreementControllerTests
{
    private EmployerAgreementController _controller;
    private Mock<EmployerAgreementOrchestrator> _orchestratorMock;
    private Mock<ICookieStorageService<FlashMessageViewModel>> _flashMessage;
    private Mock<IMediator> _mediator;

    private const string HashedAccountLegalEntityId = "AYT887";
    private const string HashedAccountId = "ABC167";
    private const string UserId = "UserNumeroUno";
    private const string HashedAgreementId = "AGREE778";
    private const string LegalEntityName = "I_Am+_Legal";

    [SetUp]
    public void Setup()
    {
        _orchestratorMock = new Mock<EmployerAgreementOrchestrator>();
        _flashMessage = new Mock<ICookieStorageService<FlashMessageViewModel>>();
        _mediator = new Mock<IMediator>();

        var httpRequestMock = new Mock<HttpRequest>();
        var httpContextMock = new Mock<HttpContext>();

        var store = new Dictionary<string, StringValues>
            { { ControllerConstants.AccountHashedIdRouteKeyName, HashedAccountId } };
        var queryCollection = new QueryCollection(store);

        httpRequestMock.Setup(x => x.Query).Returns(queryCollection);
        httpContextMock.Setup(x => x.Request).Returns(httpRequestMock.Object);

        var identity = new ClaimsIdentity(new[] { new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, UserId) });

        var principal = new ClaimsPrincipal(identity);

        httpContextMock.Setup(x => x.User).Returns(principal);

        _controller = new EmployerAgreementController(
            _orchestratorMock.Object,
            _flashMessage.Object,
            _mediator.Object,
            Mock.Of<IUrlActionHelper>());

        _controller.ControllerContext = new ControllerContext { HttpContext = httpContextMock.Object };
    }

    [Test]
    public async Task WhenRequestingConfirmRemoveOrganisationPage_AndUserIsUnauthorised_ThenAccessDeniedIsReturned()
    {
        //Arrange
        _orchestratorMock
            .Setup(x => x.GetConfirmRemoveOrganisationViewModel(HashedAccountLegalEntityId, HashedAccountId, UserId))
            .ReturnsAsync(new OrchestratorResponse<ConfirmOrganisationToRemoveViewModel>
            {
                Exception = new UnauthorizedAccessException(),
                Status = HttpStatusCode.Unauthorized
            });

        //Act
        var response = await _controller.ConfirmRemoveOrganisation(HashedAccountLegalEntityId, HashedAccountId);

        //Assert
        ViewResult viewResult = response as ViewResult;
        Assert.AreEqual(ControllerConstants.AccessDeniedViewName, viewResult.ViewName);
    }

    [Test]
    public async Task
        WhenRequestingConfirmRemoveOrganisationPage_AndOrganisationCanBeRemoved_ThenConfirmRemoveViewIsReturned()
    {
        //Arrange
        _orchestratorMock
            .Setup(x => x.GetConfirmRemoveOrganisationViewModel(HashedAccountId, HashedAccountLegalEntityId, UserId))
            .ReturnsAsync(new OrchestratorResponse<ConfirmOrganisationToRemoveViewModel>
            {
                Data = new ConfirmOrganisationToRemoveViewModel
                {
                    CanBeRemoved = true
                },
                Status = HttpStatusCode.OK
            });

        //Act
        var response = await _controller.ConfirmRemoveOrganisation(HashedAccountId, HashedAccountLegalEntityId);

        //Assert
        ViewResult viewResult = response as ViewResult;
        Assert.AreEqual(ControllerConstants.ConfirmRemoveOrganisationActionName, viewResult.ViewName);
    }

    [Test]
    public async Task
        WhenRequestingConfirmRemoveOrganisationPage_AndOrganisationCannotBeRemoved_ThenCannotRemoveOrganisationViewIsReturned()
    {
        //Arrange
        _orchestratorMock
            .Setup(x => x.GetConfirmRemoveOrganisationViewModel(HashedAccountId, HashedAccountLegalEntityId, UserId))
            .ReturnsAsync(new OrchestratorResponse<ConfirmOrganisationToRemoveViewModel>
            {
                Data = new ConfirmOrganisationToRemoveViewModel
                {
                    CanBeRemoved = false
                },
                Status = HttpStatusCode.OK
            });

        //Act
        var response = await _controller.ConfirmRemoveOrganisation(HashedAccountId, HashedAccountLegalEntityId);

        //Assert
        ViewResult viewResult = response as ViewResult;
        Assert.AreEqual(ControllerConstants.CannotRemoveOrganisationViewName, viewResult.ViewName);
    }

    [Test]
    public async Task
        ConfirmRemoveOrganisation_WhenIRemoveALegalEntityFromAnAccount_ThenTheOrchestratorIsCalledToGetTheConfirmRemoveModel()
    {
        // Arrange 
        _orchestratorMock
            .Setup(x => x.GetConfirmRemoveOrganisationViewModel(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new OrchestratorResponse<ConfirmOrganisationToRemoveViewModel>
            {
                Data = new ConfirmOrganisationToRemoveViewModel()
            });

        // Assert 
        await _controller.ConfirmRemoveOrganisation(HashedAccountId, HashedAccountLegalEntityId);

        // Assert
        _orchestratorMock.Verify(
            x => x.GetConfirmRemoveOrganisationViewModel(HashedAccountId, HashedAccountLegalEntityId, UserId),
            Times.Once);
    }

    [Test]
    public async Task
        ViewUnsignedAgreements_WhenIViewUnsignedAgreements_ThenIShouldGoStraightToTheUnsignedAgreementIfThereIsOnlyOne()
    {
        //Arrange
        _orchestratorMock
            .Setup(x => x.GetNextUnsignedAgreement(HashedAccountId, UserId))
            .ReturnsAsync(new OrchestratorResponse<NextUnsignedAgreementViewModel>
            {
                Data = new NextUnsignedAgreementViewModel
                {
                    HasNextAgreement = true,
                    NextAgreementHashedId = HashedAgreementId
                },
                Status = HttpStatusCode.OK
            });

        //Act
        var response = await _controller.ViewUnsignedAgreements(HashedAccountId);

        //Assert
        var result = response as RedirectToActionResult;
        Assert.IsNotNull(result);
        Assert.AreEqual("AboutYourAgreement", result.ActionName);
        Assert.AreEqual(HashedAgreementId, result.RouteValues["agreementId"]);
    }

    [Test, MoqAutoData]
    public async Task ViewAgreementToSign_ShouldReturnAgreements(SignEmployerAgreementViewModel viewModel)
    {
        //Arrange
        _orchestratorMock
            .Setup(x => x.GetSignedAgreementViewModel(HashedAccountId, HashedAgreementId, UserId))
            .ReturnsAsync(new OrchestratorResponse<SignEmployerAgreementViewModel>
            {
                Data = viewModel
            });

        //Act
        var response = await _controller.SignAgreement(HashedAccountId, HashedAgreementId);

        //Assert
        var result = response as ViewResult;
        result.Model.Should().BeEquivalentTo(viewModel);
    }

    [Test]
    public async Task AboutYourAgreement_WhenIViewAboutYourAgreementAsLevy_ThenShouldShowTheAboutYourAgreementView()
    {
        // Arrange
        _orchestratorMock.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new OrchestratorResponse<EmployerAgreementViewModel>
            {
                Data = new EmployerAgreementViewModel
                {
                    EmployerAgreement = new EmployerAgreementView
                    {
                        AgreementType = AgreementType.Levy
                    }
                }
            });

        // Act
        var actualResult = await _controller.AboutYourAgreement(HashedAccountId, HashedAgreementId);

        // Assert
        Assert.IsNotNull(actualResult);
        actualResult.Model.Should().BeOfType<OrchestratorResponse<EmployerAgreementViewModel>>();
        // Assert.That(actualResult.Model, Is.InstanceOf<OrchestratorResponse<EmployerAgreementViewModel>>());
    }

    [Test, MoqAutoData]
    public async Task ViewAgreementToSign_WhenIHaveNotSelectedAnOption_ThenAnErrorIsDisplayed(
        SignEmployerAgreementViewModel viewModel)
    {
        //Arrange
        _orchestratorMock
            .Setup(x => x.GetSignedAgreementViewModel(HashedAccountId, HashedAgreementId, UserId))
            .ReturnsAsync(new OrchestratorResponse<SignEmployerAgreementViewModel>
            {
                Data = viewModel
            });

        //Act
        var response = await _controller.Sign(HashedAccountId, HashedAgreementId, null);

        //Assert
        var viewResult = response as ViewResult;
        var model = viewResult.Model as SignEmployerAgreementViewModel;
        var modelState = _controller.ModelState;

        Assert.AreEqual(ControllerConstants.SignAgreementViewName, viewResult.ViewName);
        Assert.AreEqual(model, viewModel);
        Assert.IsTrue(modelState[nameof(model.Choice)].Errors.Count == 1);
    }

    [Test]
    public async Task WhenDoYouWantToView_WhenILandOnThePage_ThenTheLegalEntityNameIsCorrect()
    {
        // Arrange
        _orchestratorMock
            .Setup(x => x.GetById(HashedAgreementId, HashedAccountId, UserId))
            .ReturnsAsync(new OrchestratorResponse<EmployerAgreementViewModel>
            {
                Data = new EmployerAgreementViewModel
                {
                    EmployerAgreement = new EmployerAgreementView
                    {
                        LegalEntityName = LegalEntityName
                    }
                }
            });

        // Act
        var actualResult = await _controller.WhenDoYouWantToView(0, HashedAccountId, HashedAgreementId);

        // Assert
        actualResult.Should().NotBeNull();
        var viewResult = actualResult as ViewResult;
        viewResult.Should().NotBeNull();
        var actualModel = viewResult.Model as WhenDoYouWantToViewViewModel;
        actualModel.Should().NotBeNull();
        actualModel.EmployerAgreement.LegalEntityName.Should().Be(LegalEntityName);
    }

    [Test]
    public async Task WhenDoYouWantToView_WhenISelectNow_ThenTheAgreementIsShown()
    {
        // Act
        var actualResult = await _controller.WhenDoYouWantToView(1, HashedAgreementId, HashedAccountId) as RedirectToRouteResult;

        // Assert
        actualResult.Should().NotBeNull();
        actualResult.RouteName.Should().Be(RouteNames.EmployerAgreementSignYourAgreement);
    }

    [Test]
    public async Task WhenDoYouWantToView_WhenISelectLater_ThenTheHomepageIsShown()
    {
        var actualResult = await _controller.WhenDoYouWantToView(2, HashedAgreementId, HashedAccountId) as RedirectToRouteResult;
        actualResult.Should().NotBeNull();
        actualResult.RouteName.Should().Be(RouteNames.EmployerTeamIndex);
    }
}

public class EmployerAgreementControllerTestFixtures : FluentTest<EmployerAgreementControllerTestFixtures>
{
    public Mock<EmployerAgreementOrchestrator> Orchestrator;
    public Mock<ICookieStorageService<FlashMessageViewModel>> FlashMessage;
    public Mock<IMediator> Mediator;

    public EmployerAgreementControllerTestFixtures()
    {
        Orchestrator = new Mock<EmployerAgreementOrchestrator>();
        FlashMessage = new Mock<ICookieStorageService<FlashMessageViewModel>>();
        Mediator = new Mock<IMediator>();
        
        GetAgreementRequest = new GetEmployerAgreementRequest
        {
            ExternalUserId = UserId,
            HashedAccountId = HashedAccountId,
            HashedAgreementId = HashedAgreementId
        };

        GetAgreementToSignViewModel = new EmployerAgreementViewModel
        {
            EmployerAgreement = new EmployerAgreementView()
        };

        GetSignAgreementViewModel = new SignEmployerAgreementViewModel();
    }

    public string HashedAccountId => "ABC123";
    public string UserId => "AFV456TGF";
    public string HashedAgreementId => "789UHY";
    public long AccountLegalEntityId => 1234;
    public string LegalEntityName => "FIFTEEN LIMITED";
    public string HashedAccountLegalEntityId => "THGHFH";

    public GetEmployerAgreementRequest GetAgreementRequest { get; }

    public EmployerAgreementViewModel GetAgreementToSignViewModel { get; }

    public SignEmployerAgreementViewModel GetSignAgreementViewModel { get; }

    public ViewResult ViewResult { get; set; }

    public EmployerAgreementControllerTestFixtures WithUnsignedEmployerAgreement()
    {
        var agreementResponse = new GetEmployerAgreementResponse
        {
            EmployerAgreement = new AgreementDto { LegalEntity = new AccountSpecificLegalEntityDto { AccountLegalEntityId = AccountLegalEntityId } }
        };

        Mediator.Setup(x => x.Send(It.Is<GetEmployerAgreementRequest>(r => r.HashedAgreementId == GetAgreementRequest.HashedAgreementId && r.HashedAccountId == GetAgreementRequest.HashedAccountId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(agreementResponse);
        var entitiesCountResponse = new GetAccountLegalEntitiesCountByHashedAccountIdResponse
        {
            LegalEntitiesCount = 1
        };

        Mediator.Setup(x => x.Send(It.IsAny<GetAccountLegalEntitiesCountByHashedAccountIdRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entitiesCountResponse);

        Orchestrator.Setup(x => x.GetById(HashedAgreementId, HashedAccountId, UserId))
            .ReturnsAsync(new OrchestratorResponse<EmployerAgreementViewModel> { Data = GetAgreementToSignViewModel });

        return this;
    }

    public EmployerAgreementControllerTestFixtures WithPreviouslySignedAgreement()
    {
        var response = new GetLastSignedAgreementResponse { LastSignedAgreement = new AgreementDto() };

        Mediator.Setup(x =>
                x.Send(It.Is<GetLastSignedAgreementRequest>(r => r.AccountLegalEntityId == AccountLegalEntityId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        return this;
    }

    public EmployerAgreementController CreateController(bool isAuthenticatedUser = true)
    {
        var httpRequestMock = new Mock<HttpRequest>();
        var httpContextMock = new Mock<HttpContext>();

        var store = new Dictionary<string, StringValues> { { ControllerConstants.AccountHashedIdRouteKeyName, HashedAccountId } };
        var queryCollection = new QueryCollection(store);

        httpRequestMock.Setup(x => x.Query).Returns(queryCollection);
        httpContextMock.Setup(x => x.Request).Returns(httpRequestMock.Object);

        var identity = new ClaimsIdentity(new[] { new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, isAuthenticatedUser ? "AFV456TGF" : string.Empty) });

        var principal = new ClaimsPrincipal(identity);

        httpContextMock.Setup(x => x.User).Returns(principal);

        var controller = new EmployerAgreementController(
            Orchestrator.Object,
            FlashMessage.Object,
            Mediator.Object,
            Mock.Of<IUrlActionHelper>());

        controller.ControllerContext = new ControllerContext { HttpContext = httpContextMock.Object };

        return controller;
    }

    public Task<IActionResult> ConfirmRemoveOrganisation()
    {
        var controller = CreateController();
        return controller.ConfirmRemoveOrganisation(HashedAccountId, HashedAccountLegalEntityId);
    }

    public async Task<RedirectToActionResult> ViewUnsignedAgreements()
    {
        var controller = CreateController(false);
        var result = await controller.ViewUnsignedAgreements(HashedAccountId) as RedirectToActionResult;
        return result;
    }

    public async Task<ViewResult> SignedAgreement()
    {
        var controller = CreateController();
        ViewResult = await controller.SignAgreement(HashedAccountId, HashedAgreementId) as ViewResult;
        return ViewResult;
    }

    public async Task<Tuple<ViewResult, ModelStateDictionary>> Sign(int? choice)
    {
        var controller = CreateController();
        var result = await controller.Sign(HashedAgreementId, HashedAccountId, choice) as ViewResult;
        return new Tuple<ViewResult, ModelStateDictionary>(result, controller.ModelState);
    }

    public async Task<ViewResult> AboutYourAgreement()
    {
        var controller = CreateController();
        ViewResult = await controller.AboutYourAgreement(HashedAgreementId, HashedAccountId) as ViewResult;
        return ViewResult;
    }

    public async Task<ViewResult> WhenDoYouWantToView()
    {
        var controller = CreateController();
        ViewResult = await controller.WhenDoYouWantToView(HashedAgreementId, HashedAccountId) as ViewResult;
        return ViewResult;
    }

    public async Task<RedirectToRouteResult> WhenDoYouWantToView(int? choice, WhenDoYouWantToViewViewModel model)
    {
        var controller = CreateController();
        return await controller.WhenDoYouWantToView(choice, model.EmployerAgreement.HashedAgreementId, model.EmployerAgreement.HashedAccountId) as RedirectToRouteResult;
    }
}