using AutoMapper;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerAccounts.Dtos;
using SFA.DAS.EmployerAccounts.Exceptions;
using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;
using SFA.DAS.EmployerAccounts.Queries.GetAccountEmployerAgreements;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAgreement;
using SFA.DAS.EmployerAccounts.Queries.GetLastSignedAgreement;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Orchestrators.EmployerAgreementOrchestratorTests;

public class WhenIGetSignedAgreementViewModel
{
    private Mock<IMediator> _mediator;
    private Mock<IReferenceDataService> _referenceDataService;
    private Mock<IMapper> _mapper;
    private Mock<IEncodingService> _encodingService;
    private EmployerAgreementOrchestrator _orchestrator;

    private const string HashedAccountId = "ABC123";
    private const long AccountId = 123;
    private const string HashedAgreementId = "AGREEMENT123";
    private const string ExternalUserId = "USER1";

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _mapper = new Mock<IMapper>();
        _encodingService = new Mock<IEncodingService>();
        _referenceDataService = new Mock<IReferenceDataService>();

        _encodingService.Setup(x => x.Decode(HashedAccountId, EncodingType.AccountId)).Returns(AccountId);

        _orchestrator = new EmployerAgreementOrchestrator(
            _mediator.Object,
            _mapper.Object,
            _referenceDataService.Object,
            _encodingService.Object,
            Mock.Of<ILogger<EmployerAgreementOrchestrator>>()
        );
    }

    [Test]
    public async Task ThenTheRequestForAccountEmployerAgreementsIsMade()
    {
        // Arrange
        SetupDefaultMocks();

        // Act
        await _orchestrator.GetSignedAgreementViewModel(HashedAccountId, HashedAgreementId, ExternalUserId);

        // Assert
        _mediator.Verify(x => x.Send(It.Is<GetAccountEmployerAgreementsRequest>(
            c => c.AccountId == AccountId && c.ExternalUserId == ExternalUserId), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task ThenTheRequestForEmployerAgreementIsMade()
    {
        // Arrange
        SetupDefaultMocks();

        // Act
        await _orchestrator.GetSignedAgreementViewModel(HashedAccountId, HashedAgreementId, ExternalUserId);

        // Assert
        _mediator.Verify(x => x.Send(It.Is<GetEmployerAgreementRequest>(
            c => c.HashedAccountId == HashedAccountId 
                && c.HashedAgreementId == HashedAgreementId 
                && c.ExternalUserId == ExternalUserId), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task ThenHasAcknowledgedAgreementIsTrue_WhenAccountHasSignedAgreements()
    {
        // Arrange
        var employerAgreementsResponse = new GetAccountEmployerAgreementsResponse
        {
            EmployerAgreements =
            [
                new EmployerAgreementStatusDto
                {
                    Signed = new SignedEmployerAgreementDetailsDto { Id = 1, VersionNumber = 1 }
                },

                new EmployerAgreementStatusDto
                {
                    Pending = new EmployerAgreementDetailsDto { Id = 2, VersionNumber = 2 }
                }
            ]
        };

        SetupDefaultMocks(employerAgreementsResponse);

        // Act
        var result = await _orchestrator.GetSignedAgreementViewModel(HashedAccountId, HashedAgreementId, ExternalUserId);

        // Assert
        result.Data.HasAcknowledgedAgreement.Should().BeTrue();
    }

    [Test]
    public async Task ThenHasAcknowledgedAgreementIsFalse_WhenAccountHasNoSignedAgreements()
    {
        // Arrange
        var employerAgreementsResponse = new GetAccountEmployerAgreementsResponse
        {
            EmployerAgreements =
            [
                new EmployerAgreementStatusDto
                {
                    Pending = new EmployerAgreementDetailsDto { Id = 1, VersionNumber = 1 }
                }
            ]
        };

        SetupDefaultMocks(employerAgreementsResponse);

        // Act
        var result = await _orchestrator.GetSignedAgreementViewModel(HashedAccountId, HashedAgreementId, ExternalUserId);

        // Assert
        result.Data.HasAcknowledgedAgreement.Should().BeFalse();
    }

    [Test]
    public async Task ThenHasAcknowledgedAgreementIsFalse_WhenEmployerAgreementsIsNull()
    {
        // Arrange
        var employerAgreementsResponse = new GetAccountEmployerAgreementsResponse
        {
            EmployerAgreements = null
        };

        SetupDefaultMocks(employerAgreementsResponse);

        // Act
        var result = await _orchestrator.GetSignedAgreementViewModel(HashedAccountId, HashedAgreementId, ExternalUserId);

        // Assert
        result.Data.HasAcknowledgedAgreement.Should().BeFalse();
    }

    [Test]
    public async Task ThenHasAcknowledgedAgreementIsFalse_WhenEmployerAgreementsIsEmpty()
    {
        // Arrange
        var employerAgreementsResponse = new GetAccountEmployerAgreementsResponse
        {
            EmployerAgreements = []
        };

        SetupDefaultMocks(employerAgreementsResponse);

        // Act
        var result = await _orchestrator.GetSignedAgreementViewModel(HashedAccountId, HashedAgreementId, ExternalUserId);

        // Assert
        result.Data.HasAcknowledgedAgreement.Should().BeFalse();
    }

    [Test]
    public async Task ThenHasAcknowledgedAgreementIsTrue_WhenAccountHasMultipleSignedAgreements()
    {
        // Arrange
        var employerAgreementsResponse = new GetAccountEmployerAgreementsResponse
        {
            EmployerAgreements =
            [
                new EmployerAgreementStatusDto
                {
                    Signed = new SignedEmployerAgreementDetailsDto { Id = 1, VersionNumber = 1 }
                },

                new EmployerAgreementStatusDto
                {
                    Signed = new SignedEmployerAgreementDetailsDto { Id = 2, VersionNumber = 2 }
                },

                new EmployerAgreementStatusDto
                {
                    Pending = new EmployerAgreementDetailsDto { Id = 3, VersionNumber = 3 }
                }
            ]
        };

        SetupDefaultMocks(employerAgreementsResponse);

        // Act
        var result = await _orchestrator.GetSignedAgreementViewModel(HashedAccountId, HashedAgreementId, ExternalUserId);

        // Assert
        result.Data.HasAcknowledgedAgreement.Should().BeTrue();
    }

    [Test]
    public async Task ThenIfAnInvalidRequestExceptionIsThrownTheOrchestratorResponseContainsTheError()
    {
        // Arrange
        _mediator.Setup(x => x.Send(It.IsAny<GetAccountEmployerAgreementsRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidRequestException(new Dictionary<string, string>()));

        // Act
        var actual = await _orchestrator.GetSignedAgreementViewModel(HashedAccountId, HashedAgreementId, ExternalUserId);

        // Assert
        actual.Status.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ThenIfAUnauthorizedAccessExceptionIsThrownThenTheOrchestratorResponseShowsAccessDenied()
    {
        // Arrange
        _mediator.Setup(x => x.Send(It.IsAny<GetAccountEmployerAgreementsRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act
        var actual = await _orchestrator.GetSignedAgreementViewModel(HashedAccountId, HashedAgreementId, ExternalUserId);

        // Assert
        actual.Status.Should().Be(HttpStatusCode.Unauthorized);
    }

    private void SetupDefaultMocks()
    {
        SetupDefaultMocks(new GetAccountEmployerAgreementsResponse { EmployerAgreements = [] });
    }

    private void SetupDefaultMocks(GetAccountEmployerAgreementsResponse employerAgreementsResponse)
    {
        var getEmployerAgreementResponse = new GetEmployerAgreementResponse
        {
            EmployerAgreement = new AgreementDto
            {
                LegalEntity = new AccountSpecificLegalEntityDto
                {
                    AccountLegalEntityId = 1
                }
            }
        };

        var getLastSignedAgreementResponse = new GetLastSignedAgreementResponse
        {
            LastSignedAgreement = null
        };

        var signEmployerAgreementViewModel = new SignEmployerAgreementViewModel();

        _mediator.Setup(x => x.Send(It.IsAny<GetAccountEmployerAgreementsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employerAgreementsResponse);

        _mediator.Setup(x => x.Send(It.IsAny<GetEmployerAgreementRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(getEmployerAgreementResponse);

        _mediator.Setup(x => x.Send(It.IsAny<GetLastSignedAgreementRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(getLastSignedAgreementResponse);

        _mapper.Setup(m => m.Map<GetEmployerAgreementResponse, SignEmployerAgreementViewModel>(It.IsAny<GetEmployerAgreementResponse>()))
            .Returns(signEmployerAgreementViewModel);

        _mapper.Setup(m => m.Map<EmployerAgreementView>(It.IsAny<AgreementDto>()))
            .Returns((EmployerAgreementView)null);
    }
}
