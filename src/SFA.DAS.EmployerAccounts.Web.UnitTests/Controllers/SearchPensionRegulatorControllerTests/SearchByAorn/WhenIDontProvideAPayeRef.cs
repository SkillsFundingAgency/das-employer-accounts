using MediatR;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.SearchPensionRegulatorControllerTests.SearchByAorn;

[TestFixture]
class WhenIDontProvideAPayeRef
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
        _controller.Dispose();
    }

    [TestCase("")]
    [TestCase(null)]
    public async Task ThenAnErrorIsDisplayed(string payeRef)
    {
        var response = await _controller.SearchPensionRegulatorByAorn(new SearchPensionRegulatorByAornViewModel { Aorn = "1234567890ABC", PayeRef = payeRef });
        var viewResponse = (ViewResult)response;

        Assert.That(viewResponse.ViewName, Is.EqualTo(ControllerConstants.SearchUsingAornViewName));
        var viewModel = viewResponse.Model as SearchPensionRegulatorByAornViewModel;
        Assert.That(viewModel.PayeRefError, Is.EqualTo("Enter your PAYE reference in the correct format"));
    }
}