using MediatR;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.SearchPensionRegulatorControllerTests.SearchByAorn;

[TestFixture]
class WhenIDontProvideAValidAorn
{
    private SearchPensionRegulatorController _controller;
        
    [SetUp]
    public void Setup()
    {
        _controller = new SearchPensionRegulatorController(
            Mock.Of<SearchPensionRegulatorOrchestrator>(),
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
    public async Task ThenAnErrorIsDisplayed()
    {
        var response = await _controller.SearchPensionRegulatorByAorn(new SearchPensionRegulatorByAornViewModel { Aorn = "SDCXDD", PayeRef = "000/EDDEFDS" });
        var viewResponse = (ViewResult)response;

        Assert.That(viewResponse.ViewName, Is.EqualTo(ControllerConstants.SearchUsingAornViewName));
        var viewModel = viewResponse.Model as SearchPensionRegulatorByAornViewModel;
        Assert.That(viewModel.AornError, Is.EqualTo("Enter your Accounts Office reference in the correct format"));
    }
}