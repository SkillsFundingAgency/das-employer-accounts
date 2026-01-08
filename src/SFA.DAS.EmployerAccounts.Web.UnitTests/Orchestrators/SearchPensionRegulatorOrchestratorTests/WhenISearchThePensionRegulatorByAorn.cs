using System.Diagnostics.CodeAnalysis;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Models.Organisation;
using SFA.DAS.EmployerAccounts.Models.PensionRegulator;
using SFA.DAS.EmployerAccounts.Queries.GetOrganisationsByAorn;
using SFA.DAS.EmployerAccounts.Queries.GetPensionRegulator;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Orchestrators.SearchPensionRegulatorOrchestratorTests;

[ExcludeFromCodeCoverage]
public class WhenISearchThePensionRegulatorByAorn
{
    private SearchPensionRegulatorOrchestrator _orchestrator;
    private Mock<IMediator> _mediator;
      
    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
          
        _mediator.Setup(x => x.Send(It.IsAny<GetPensionRegulatorRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetPensionRegulatorResponse
            {
                Organisations = new List<Organisation>()
            });

        _orchestrator = new SearchPensionRegulatorOrchestrator(
            _mediator.Object, 
            Mock.Of<ICookieStorageService<EmployerAccountData>>(),
            Mock.Of<ILogger<SearchPensionRegulatorOrchestrator>>());
    }

    [Test]
    public async Task ThenMatchingOrganisationsAreReturned()
    {
        //Arrange
        var payeRef = "123/4567";
        var aorn = "aorn";
        var queryResponse = new GetOrganisationsByAornResponse { Organisations = new List<Organisation> { new Organisation { Address = new Address() }, new Organisation { Address = new Address() } } };

        _mediator.Setup(x => x.Send(It.Is<GetOrganisationsByAornRequest>(c => c.PayeRef.Equals(payeRef) && c.Aorn.Equals(aorn)), It.IsAny<CancellationToken>())).ReturnsAsync(queryResponse);

        //Act
        var response = await _orchestrator.GetOrganisationsByAorn(aorn, payeRef);

        //Assert
        response.Data.PayeRef.Should().Be(payeRef);
        response.Data.Aorn.Should().Be(aorn);
        response.Data.Results.Count.Should().Be(queryResponse.Organisations.Count());
    }

    [Test]
    public async Task IfGettingOrganisationsFailsThenNoOrganisationsAreReturned()
    {
        //Arrange
        var payeRef = "123/4567";
        var aorn = "aorn";
            
        _mediator.Setup(x => x.Send(It.Is<GetOrganisationsByAornRequest>(c => c.PayeRef.Equals(payeRef) && c.Aorn.Equals(aorn)), It.IsAny<CancellationToken>())).Throws(new Exception());

        //Act
        var response = await _orchestrator.GetOrganisationsByAorn(aorn, payeRef);

        //Assert
        response.Data.PayeRef.Should().Be(payeRef);
        response.Data.Aorn.Should().Be(aorn);
        response.Data.Results.Count.Should().Be(0);
    }

    [Test]
    public async Task ThenEachResultIsCorrectlyMarkedAsComingFromPensionsRegulator()
    {
        var actual = await _orchestrator.SearchPensionRegulator(It.IsAny<string>());

        actual
            .Data
            .Results
            .All(organisation => organisation.Type == OrganisationType.PensionsRegulator)
            .Should().BeTrue();
    }
}