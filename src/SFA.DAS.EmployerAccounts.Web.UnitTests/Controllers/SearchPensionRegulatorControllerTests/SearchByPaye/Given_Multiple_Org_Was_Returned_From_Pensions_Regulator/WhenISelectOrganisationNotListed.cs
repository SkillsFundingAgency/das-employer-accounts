using MediatR;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.SearchPensionRegulatorControllerTests.SearchByPaye.Given_Multiple_Org_Was_Returned_From_Pensions_Regulator;

[TestFixture]
class WhenISelectOrganisationNotListed
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
    public void ThenTheSearchOrganisationPageIsDisplayed()
    {
        var viewModel = new SearchPensionRegulatorResultsViewModel
        {
            Results = new List<PensionRegulatorDetailsViewModel>
            {
                new PensionRegulatorDetailsViewModel { ReferenceNumber = 1 },
                new PensionRegulatorDetailsViewModel { ReferenceNumber = 2 }
            },
            SelectedOrganisation = 0
        };

        var response = _controller.SearchPensionRegulator(It.IsAny<string>(), viewModel).Result;
        var redirectResponse = (RedirectToActionResult) response;

        Assert.That(redirectResponse.ActionName, Is.EqualTo(ControllerConstants.SearchForOrganisationActionName));
        Assert.That(redirectResponse.ControllerName, Is.EqualTo(ControllerConstants.SearchOrganisationControllerName));
    }
}