﻿using MediatR;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.SearchPensionRegulatorControllerTests.SearchByPaye.Given_Multiple_Org_Was_Returned_From_Pensions_Regulator;

[TestFixture]
class WhenISearchThePensionRegulator
{
    private SearchPensionRegulatorController _controller;    
       
    [SetUp]
    public void Setup()
    {                   
        var orchestrator = new Mock<SearchPensionRegulatorOrchestrator>();

        orchestrator
            .Setup(x => x.SearchPensionRegulator(It.IsAny<string>()))
            .ReturnsAsync(
                new OrchestratorResponse<SearchPensionRegulatorResultsViewModel>
                {
                    Data = new SearchPensionRegulatorResultsViewModel
                    {
                        Results = new List<PensionRegulatorDetailsViewModel>
                        {
                            new PensionRegulatorDetailsViewModel(),
                            new PensionRegulatorDetailsViewModel()
                        }
                    }
                });

        orchestrator.Setup(x => x.GetCookieData())
            .Returns(
                new EmployerAccountData
                {
                    EmployerAccountPayeRefData = new EmployerAccountPayeRefData
                    {
                        PayeReference = "PayeRef"
                    }
                });

        _controller = new SearchPensionRegulatorController(
            orchestrator.Object,
            Mock.Of<ICookieStorageService<FlashMessageViewModel>>(),
            Mock.Of<IMediator>(),
            Mock.Of<ICookieStorageService<HashedAccountIdModel>>());
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    [Test]
    public async Task ThenThePensionRegulatorResultsPageIsDisplayed()
    {
        var response = await _controller.SearchPensionRegulator(It.IsAny<string>());
        var viewResponse = (ViewResult) response;

        Assert.That(viewResponse.ViewName, Is.EqualTo(ControllerConstants.SearchPensionRegulatorResultsViewName));
    }
}