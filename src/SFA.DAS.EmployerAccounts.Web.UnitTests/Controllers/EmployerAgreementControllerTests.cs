using System.Security.Claims;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Commands.AcknowledgeEmployerAgreement;
using SFA.DAS.EmployerAccounts.Infrastructure;
using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;
using SFA.DAS.EmployerAccounts.Web.RouteValues;
using SFA.DAS.Encoding;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers;

[TestFixture]
public class EmployerAgreementControllerTests
{
    private EmployerAgreementController _controller;
    private Mock<EmployerAgreementOrchestrator> _orchestratorMock;
    private Mock<ICookieStorageService<FlashMessageViewModel>> _flashMessage;
    private Mock<IMediator> _mediator;
    private Mock<HttpContext> _httpContextMock;

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
        _httpContextMock = new Mock<HttpContext>();

        var store = new Dictionary<string, StringValues>
            { { ControllerConstants.AccountHashedIdRouteKeyName, HashedAccountId } };
        var queryCollection = new QueryCollection(store);

        httpRequestMock.Setup(x => x.Query).Returns(queryCollection);
        _httpContextMock.Setup(x => x.Request).Returns(httpRequestMock.Object);

        var identity = new ClaimsIdentity(new[] { new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier, UserId) });

        var principal = new ClaimsPrincipal(identity);

        _httpContextMock.Setup(x => x.User).Returns(principal);

        _controller = new EmployerAgreementController(
            _orchestratorMock.Object,
            _flashMessage.Object,
            _mediator.Object,
            Mock.Of<IUrlActionHelper>());

        _controller.ControllerContext = new ControllerContext { HttpContext = _httpContextMock.Object };
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
    
    [Test, MoqAutoData]
    public async Task SignAgreement_Later_ShouldAcknowledgeAgreement(
        long agreementId,
        [Frozen] Mock<IEncodingService> encodingServiceMock,
        [Frozen] Mock<IMediator> mediatorMock,
        [NoAutoProperties] EmployerAgreementController controller)
    {
        // Arrange 
        controller.ControllerContext = new ControllerContext { HttpContext = _httpContextMock.Object };
        
        encodingServiceMock
            .Setup(es => es.Decode(HashedAgreementId, EncodingType.AccountId))
            .Returns(agreementId);
        
        // Act
        var actualResult = await controller.Sign(HashedAccountId, HashedAgreementId, 1) as RedirectToRouteResult;
        
        // Assert
        mediatorMock.Verify(m => 
            m.Send(It.IsAny<AcknowledgeEmployerAgreementCommand>(), 
                It.IsAny<CancellationToken>()));
        actualResult.RouteName.Should().Be(RouteNames.EmployerTeamIndex);
    }
}