using MediatR;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.SearchPensionRegulatorControllerTests.SearchByAorn;

[TestFixture]
class WhenIDontProvideAnAorn
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

    [TestCase("")]
    [TestCase(null)]
    public async Task ThenAnErrorIsDisplayed(string aorn)
    {
        var response = await _controller.SearchPensionRegulatorByAorn(new SearchPensionRegulatorByAornViewModel { Aorn = aorn, PayeRef = "000/EDDEFDS" });
        var viewResponse = (ViewResult)response;

        viewResponse.ViewName.Should().Be(ControllerConstants.SearchUsingAornViewName);
        var viewModel = viewResponse.Model as SearchPensionRegulatorByAornViewModel;
        viewModel.AornError.Should().Be("Enter your Accounts Office reference in the correct format");
    }
}