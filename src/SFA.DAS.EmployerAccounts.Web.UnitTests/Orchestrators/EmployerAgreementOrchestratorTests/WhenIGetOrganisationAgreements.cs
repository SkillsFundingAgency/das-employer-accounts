using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerAccounts.Dtos;
using SFA.DAS.EmployerAccounts.Exceptions;
using SFA.DAS.EmployerAccounts.Queries.GetOrganisationAgreements;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Orchestrators.EmployerAgreementOrchestratorTests;

public class WhenIGetOrganisationAgreements
{
    private Mock<IMediator> _mediator;
    private Mock<IReferenceDataService> _referenceDataService;
    private Mock<IMapper> _mapper;
    private EmployerAgreementOrchestrator _orchestrator;

    public string AccountLegalEntityHashedId = "2K7J94";

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _mapper = new Mock<IMapper>();
        _mediator.Setup(x => x.Send(It.IsAny<GetOrganisationAgreementsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetOrganisationAgreementsResponse
            {
                Agreements = new List<EmployerAgreementDto>()
                {
                    new EmployerAgreementDto { SignedDate = DateTime.UtcNow }
                }
            });             

        var organisationAgreementViewModel = new List<OrganisationAgreementViewModel>()
        {
            new OrganisationAgreementViewModel { SignedDate = DateTime.UtcNow }
        };

        _referenceDataService = new Mock<IReferenceDataService>();
        _mapper.Setup(m => m.Map<ICollection<EmployerAgreementDto>, ICollection<OrganisationAgreementViewModel>>(It.IsAny<ICollection<EmployerAgreementDto>>())).Returns(organisationAgreementViewModel);
        _orchestrator = new EmployerAgreementOrchestrator(
            _mediator.Object,
            _mapper.Object,
            _referenceDataService.Object,
            Mock.Of<IEncodingService>(),
            Mock.Of<ILogger<EmployerAgreementOrchestrator>>()
            );
    }

    [Test]
    public async Task ThenTheRequestForAllOrganisationAgreementsIsMadeForAccountLegalEntity()
    {

        //Act
        await _orchestrator.GetOrganisationAgreements(AccountLegalEntityHashedId);

        //Assert
        _mediator.Verify(x => x.Send(It.Is<GetOrganisationAgreementsRequest>(
            c => c.AccountLegalEntityHashedId.Equals(AccountLegalEntityHashedId)), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task ThenIfAnInvalidRequestExceptionIsThrownTheOrchestratorResponseContainsTheError()
    {
        //Arrange
        _mediator.Setup(x => x.Send(It.IsAny<GetOrganisationAgreementsRequest>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidRequestException(new Dictionary<string, string>()));

        //Act
        var actual = await _orchestrator.GetOrganisationAgreements(AccountLegalEntityHashedId);

        //Assert
        Assert.That(actual.Status, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ThenIfAUnauthroizedAccessExceptionIsThrownThenTheOrchestratorResponseShowsAccessDenied()
    {
        //Arrange
        _mediator.Setup(x => x.Send(It.IsAny<GetOrganisationAgreementsRequest>(), It.IsAny<CancellationToken>())).ThrowsAsync(new UnauthorizedAccessException());

        //Act
        var actual = await _orchestrator.GetOrganisationAgreements(AccountLegalEntityHashedId);

        //Assert
        Assert.That(actual.Status, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ThenTheValuesAreReturnedInTheResponseFromTheRequestForAllOrganisationAgreementsIsMadeForAccountLegalEntity()
    {
        //Act
        var actual = await _orchestrator.GetOrganisationAgreements(AccountLegalEntityHashedId);

        //Assert
        Assert.That(actual.Data.Agreements.Any(), Is.True);            
    }
}