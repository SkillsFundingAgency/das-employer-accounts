﻿using MediatR;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.SearchPensionRegulatorControllerTests.SearchByPaye.Given_Multiple_Org_Was_Returned_From_Pensions_Regulator;

[TestFixture]
class WhenIDontSelectAnOrganisation
{
    private SearchPensionRegulatorController _controller;

    [SetUp]
    public void Setup()
    {
        var orchestrator = new Mock<SearchPensionRegulatorOrchestrator>();

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
    public void ThenThePensionRegulatorResultsPageIsDisplayed()
    {
        var viewModel = new SearchPensionRegulatorResultsViewModel
        {
            Results = new List<PensionRegulatorDetailsViewModel>
            {
                new PensionRegulatorDetailsViewModel { ReferenceNumber = 1 },
                new PensionRegulatorDetailsViewModel { ReferenceNumber = 2 }
            }
        };

        var response = _controller.SearchPensionRegulator(It.IsAny<string>(), viewModel).Result;
        var viewResponse = (ViewResult)response;

        Assert.That(viewResponse.ViewName, Is.EqualTo(ControllerConstants.SearchPensionRegulatorResultsViewName));
        Assert.That(viewResponse.ViewData["InError"], Is.EqualTo(true));
    }
}