using System.Diagnostics.CodeAnalysis;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Models.PensionRegulator;
using SFA.DAS.EmployerAccounts.Queries.GetPensionRegulator;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Orchestrators.SearchPensionRegulatorOrchestratorTests;

[ExcludeFromCodeCoverage]
public class WhenISearchThePensionRegulator
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
    public async Task ThenTheMediatorIsCalledToGetThePensionRegulatorResult()
    {
        //Arrange
        var payeRef = "123/4567";
          
        //Act
        await _orchestrator.SearchPensionRegulator(payeRef);

        //Assert
        _mediator.Verify(x => x.Send(It.Is<GetPensionRegulatorRequest>(c => c.PayeRef.Equals(payeRef)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ThenThePensionRegulatorOrganisationListIsReturnedInTheResult()
    {          
        //Act
        var actual = await _orchestrator.SearchPensionRegulator(It.IsAny<string>());

        //Assert
        actual.Should().BeAssignableTo<OrchestratorResponse<SearchPensionRegulatorResultsViewModel>>();
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