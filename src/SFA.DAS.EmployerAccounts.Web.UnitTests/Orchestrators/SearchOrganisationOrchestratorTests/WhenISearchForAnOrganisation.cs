using MediatR;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Exceptions;
using SFA.DAS.EmployerAccounts.Models.Organisation;
using SFA.DAS.EmployerAccounts.Models.ReferenceData;
using SFA.DAS.EmployerAccounts.Queries.GetAccountLegalEntities;
using SFA.DAS.EmployerAccounts.Queries.GetOrganisations;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Orchestrators.SearchOrganisationOrchestratorTests;

public class WhenISearchForAnOrganisation
{
    private SearchOrganisationOrchestrator _orchestrator;
    private Mock<IMediator> _mediator;
    private Mock<ICookieStorageService<EmployerAccountData>> _cookieService;



    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _cookieService = new Mock<ICookieStorageService<EmployerAccountData>>();

        _mediator.Setup(x => x.Send(It.IsAny<GetOrganisationsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetOrganisationsResponse { Organisations = new PagedResponse<OrganisationName> { Data = new List<OrganisationName>() } });

        _orchestrator = new SearchOrganisationOrchestrator(_mediator.Object, _cookieService.Object);
    }

    [Test]
    public async Task ThenTheMediatorIsCalledToGetTheOrganisationResult()
    {
        //Arrange
        var searchTerm = "Test Org";
        var pageNumber = 2;
        var organisationType = OrganisationType.Charities;

        //Act
        await _orchestrator.SearchOrganisation(searchTerm, pageNumber, organisationType, null, null);


        //Assert
        _mediator.Verify(x => x.Send(It.Is<GetOrganisationsRequest>(c => c.SearchTerm.Equals(searchTerm) && c.PageNumber == pageNumber && c.OrganisationType == organisationType), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ThenTheOrganisationListIsReturnedInTheResult()
    {
        //Arrange
        var searchTerm = "Test Org";
        var pageNumber = 1;
        var organisationType = OrganisationType.CompaniesHouse;

        //Act
        var actual = await _orchestrator.SearchOrganisation(searchTerm, pageNumber, organisationType, null, null);

        //Assert
        actual.Should().BeAssignableTo<OrchestratorResponse<SearchOrganisationResultsViewModel>>();
    }

    [Test]
    public async Task ThenAnInvalidRequestExceptionIsHandledTheOrchestratorResponseIsSetToBadRequest()
    {
        //Arrange
        _mediator.Setup(x => x.Send(It.IsAny<GetOrganisationsRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidRequestException(new Dictionary<string, string> {{"", ""}}));

        //Act
        var actual = await _orchestrator.SearchOrganisation("Test", 1, OrganisationType.Charities, null, null);

        //Assert
        actual.Status.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ThenIfAnOrganisationIsNotAlreadyAddedToTheAccountThenItIsSelectable()
    {
        //Arrange
        var hashedAccountId = "ABC123";
        var userId = "test";
        var searchTerm = "Test Org";
        var pageNumber = 2;
        var expectedLegalEntitiesResponse = new GetAccountLegalEntitiesResponse
        {
            LegalEntities = new List<AccountSpecificLegalEntity> { new AccountSpecificLegalEntity { Source = OrganisationType.CompaniesHouse, Code = "zzz999" } } 
        };
        _mediator.Setup(x => x.Send(It.Is<GetAccountLegalEntitiesRequest>(y => y.HashedLegalEntityId == hashedAccountId && y.UserId == userId), It.IsAny<CancellationToken>())).ReturnsAsync(expectedLegalEntitiesResponse);
        var expectedSearchResults = new GetOrganisationsResponse
        {
            Organisations = new PagedResponse<OrganisationName> { Data = new List<OrganisationName> { new OrganisationName { Type = OrganisationType.CompaniesHouse, Code = "123456789", Address = new Address() } } }
        };
        _mediator.Setup(x => x.Send(It.Is<GetOrganisationsRequest>(c => c.SearchTerm.Equals(searchTerm) && c.PageNumber == pageNumber), It.IsAny<CancellationToken>())).ReturnsAsync(expectedSearchResults);

        //Act
        var result = await _orchestrator.SearchOrganisation(searchTerm, pageNumber, null, hashedAccountId, userId);

        //Assert
        result.Data.Results.Data.Single().AddedToAccount.Should().BeFalse();
    }

    [Test]
    public async Task ThenIfACompanyIsAlreadyAddedToTheAccountThenItIsNotSelectable()
    {
        //Arrange
        var hashedAccountId = "ABC123";
        var userId = "test";
        var companyCode = "zzz9435";
        var searchTerm = "Test Org";
        var pageNumber = 2;
        var expectedLegalEntitiesResponse = new GetAccountLegalEntitiesResponse
        {
            LegalEntities = new List<AccountSpecificLegalEntity> { new AccountSpecificLegalEntity { Source = OrganisationType.CompaniesHouse, Code = companyCode } } 
        };
        _mediator.Setup(x => x.Send(It.Is<GetAccountLegalEntitiesRequest>(y => y.HashedLegalEntityId == hashedAccountId && y.UserId == userId), It.IsAny<CancellationToken>())).ReturnsAsync(expectedLegalEntitiesResponse);
        var expectedSearchResults = new GetOrganisationsResponse
        {
            Organisations = new PagedResponse<OrganisationName> { Data = new List<OrganisationName> { new OrganisationName { Type = OrganisationType.CompaniesHouse, Code = companyCode, Address = new Address() } } }
        };
        _mediator.Setup(x => x.Send(It.Is<GetOrganisationsRequest>(c => c.SearchTerm.Equals(searchTerm) && c.PageNumber == pageNumber), It.IsAny<CancellationToken>())).ReturnsAsync(expectedSearchResults);

        //Act
        var result = await _orchestrator.SearchOrganisation(searchTerm, pageNumber, null, hashedAccountId, userId);

        //Assert
        result.Data.Results.Data.Single().AddedToAccount.Should().BeTrue();
    }

    [Test]
    public async Task ThenIfACharityIsAlreadyAddedToTheAccountThenItIsNotSelectable()
    {
        //Arrange
        var hashedAccountId = "ABC123";
        var userId = "test";
        var charityCode = "zzz9435";
        var searchTerm = "Test Org";
        var pageNumber = 2;
        var expectedLegalEntitiesResponse = new GetAccountLegalEntitiesResponse
        {
            LegalEntities = new List<AccountSpecificLegalEntity> { new AccountSpecificLegalEntity { Source = OrganisationType.Charities, Code = charityCode } }
        };
        _mediator.Setup(x => x.Send(It.Is<GetAccountLegalEntitiesRequest>(y => y.HashedLegalEntityId == hashedAccountId && y.UserId == userId), It.IsAny<CancellationToken>())).ReturnsAsync(expectedLegalEntitiesResponse);
        var expectedSearchResults = new GetOrganisationsResponse
        {
            Organisations = new PagedResponse<OrganisationName> { Data = new List<OrganisationName> { new OrganisationName { Type = OrganisationType.Charities, Code = charityCode, Address = new Address() } } }
        };
        _mediator.Setup(x => x.Send(It.Is<GetOrganisationsRequest>(c => c.SearchTerm.Equals(searchTerm) && c.PageNumber == pageNumber), It.IsAny<CancellationToken>())).ReturnsAsync(expectedSearchResults);

        //Act
        var result = await _orchestrator.SearchOrganisation(searchTerm, pageNumber, null, hashedAccountId, userId);

        //Assert
        result.Data.Results.Data.Single().AddedToAccount.Should().BeTrue();
    }

    [Test]
    public async Task ThenIfAPublicSectorOrganisationIsAlreadyAddedToTheAccountThenItIsNotSelectable()
    {
        //Arrange
        var hashedAccountId = "ABC123";
        var userId = "test";
        var organisationName = "Org Name";
        var searchTerm = "Test Org";
        var pageNumber = 2;
        var expectedLegalEntitiesResponse = new GetAccountLegalEntitiesResponse
        {
            LegalEntities = new List<AccountSpecificLegalEntity> { new AccountSpecificLegalEntity { Source = OrganisationType.PublicBodies, Name = organisationName } } 
        };
        _mediator.Setup(x => x.Send(It.Is<GetAccountLegalEntitiesRequest>(y => y.HashedLegalEntityId == hashedAccountId && y.UserId == userId), It.IsAny<CancellationToken>())).ReturnsAsync(expectedLegalEntitiesResponse);
        var expectedSearchResults = new GetOrganisationsResponse
        {
            Organisations = new PagedResponse<OrganisationName> { Data = new List<OrganisationName> { new OrganisationName { Type = OrganisationType.PublicBodies, Name = organisationName, Address = new Address() } } }
        };
        _mediator.Setup(x => x.Send(It.Is<GetOrganisationsRequest>(c => c.SearchTerm.Equals(searchTerm) && c.PageNumber == pageNumber), It.IsAny<CancellationToken>())).ReturnsAsync(expectedSearchResults);

        //Act
        var result = await _orchestrator.SearchOrganisation(searchTerm, pageNumber, null, hashedAccountId, userId);

        //Assert
        result.Data.Results.Data.Single().AddedToAccount.Should().BeTrue();
    }

    [Test]
    public async Task ThenIfAnOtherOrganisationIsAlreadyAddedToTheAccountThenItIsNotSelectable()
    {
        //Arrange
        var hashedAccountId = "ABC123";
        var userId = "test";
        var organisationName = "Org Name";
        var searchTerm = "Test Org";
        var pageNumber = 2;
        var expectedLegalEntitiesResponse = new GetAccountLegalEntitiesResponse
        {
            LegalEntities = new List<AccountSpecificLegalEntity> { new AccountSpecificLegalEntity { Source = OrganisationType.Other, Name = organisationName } } 
        };
        _mediator.Setup(x => x.Send(It.Is<GetAccountLegalEntitiesRequest>(y => y.HashedLegalEntityId == hashedAccountId && y.UserId == userId), It.IsAny<CancellationToken>())).ReturnsAsync(expectedLegalEntitiesResponse);
        var expectedSearchResults = new GetOrganisationsResponse
        {
            Organisations = new PagedResponse<OrganisationName> { Data = new List<OrganisationName> { new OrganisationName { Type = OrganisationType.Other, Name = organisationName, Address = new Address() } } }
        };
        _mediator.Setup(x => x.Send(It.Is<GetOrganisationsRequest>(c => c.SearchTerm.Equals(searchTerm) && c.PageNumber == pageNumber), It.IsAny<CancellationToken>())).ReturnsAsync(expectedSearchResults);

        //Act
        var result = await _orchestrator.SearchOrganisation(searchTerm, pageNumber, null, hashedAccountId, userId);

        //Assert
        result.Data.Results.Data.Single().AddedToAccount.Should().BeTrue();
    }

    [Test]
    public async Task ThenIfAPensionsRegulatorOrganisationIsAlreadyAddedToTheAccountThenItIsNotSelectable()
    {
        //Arrange
        var hashedAccountId = "ABC123";
        var userId = "test";
        var companyCode = "zzz9435";
        var searchTerm = "Test Org";
        var pageNumber = 2;
        var expectedLegalEntitiesResponse = new GetAccountLegalEntitiesResponse
        {
            LegalEntities = new List<AccountSpecificLegalEntity> { new AccountSpecificLegalEntity { Source = OrganisationType.PensionsRegulator, Code = companyCode } }
        };
        _mediator.Setup(x => x.Send(It.Is<GetAccountLegalEntitiesRequest>(y => y.HashedLegalEntityId == hashedAccountId && y.UserId == userId), It.IsAny<CancellationToken>())).ReturnsAsync(expectedLegalEntitiesResponse);
        var expectedSearchResults = new GetOrganisationsResponse
        {
            Organisations = new PagedResponse<OrganisationName> { Data = new List<OrganisationName> { new OrganisationName { Type = OrganisationType.PensionsRegulator, Code = companyCode, Address = new Address() } } }
        };
        _mediator.Setup(x => x.Send(It.Is<GetOrganisationsRequest>(c => c.SearchTerm.Equals(searchTerm) && c.PageNumber == pageNumber), It.IsAny<CancellationToken>())).ReturnsAsync(expectedSearchResults);

        //Act
        var result = await _orchestrator.SearchOrganisation(searchTerm, pageNumber, null, hashedAccountId, userId);

        //Assert
        result.Data.Results.Data.Single().AddedToAccount.Should().BeTrue();
    }

    [Test]
    public async Task ThenIfThereIsNoDasAccountThenExistingOrganisationsArentChecked()
    {
        //Act
        await _orchestrator.SearchOrganisation("Test Org", 1, null, null, null);

        //Assert
        _mediator.Verify(x => x.Send(It.IsAny<GetAccountLegalEntitiesRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}