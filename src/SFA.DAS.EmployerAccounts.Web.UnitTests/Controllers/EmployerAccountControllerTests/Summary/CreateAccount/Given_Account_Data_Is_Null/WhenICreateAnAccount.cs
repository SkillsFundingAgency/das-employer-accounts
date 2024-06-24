using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerAccountControllerTests.Summary.CreateAccount.Given_Account_Data_Is_Null;

class WhenICreateAnAccount : ControllerTestBase
{
    private EmployerAccountController _employerAccountController;
    private Mock<EmployerAccountOrchestrator> _orchestrator;
    private const string ExpectedRedirectUrl = "http://redirect.local.test";
    private Mock<ICookieStorageService<FlashMessageViewModel>> _flashMessage;
    private SummaryViewModel _summaryViewModel;

    [SetUp]
    public void Arrange()
    {
        base.Arrange(ExpectedRedirectUrl);

        _summaryViewModel = new SummaryViewModel { IsOrganisationWithCorrectAddress = true };

        _orchestrator = new Mock<EmployerAccountOrchestrator>();

        var logger = new Mock<ILogger<EmployerAccountController>>();
        _flashMessage = new Mock<ICookieStorageService<FlashMessageViewModel>>();

        _orchestrator.Setup(x => x.GetCookieData())
            .Returns((EmployerAccountData)null);

        _employerAccountController = new EmployerAccountController(
            _orchestrator.Object,
            logger.Object,
            _flashMessage.Object,
            Mock.Of<IMediator>(),
            Mock.Of<ICookieStorageService<ReturnUrlModel>>(),
            Mock.Of<ICookieStorageService<HashedAccountIdModel>>(),
            Mock.Of<LinkGenerator>())
        {
            ControllerContext = ControllerContext,
            Url = new UrlHelper(new ActionContext(MockHttpContext.Object, Routes, new ActionDescriptor()))
        };
    }

    [TearDown]
    public void TearDown()
    {
        _employerAccountController?.Dispose();
    }

    [Test]
    public async Task Then_I_Should_Be_Redirected_To_Search_Organisatoin_Page()
    {
        //Act
        var result = await _employerAccountController.Summary(_summaryViewModel) as RedirectToActionResult;

        //Assert
        Assert.That(result.ActionName, Is.EqualTo(ControllerConstants.SearchForOrganisationActionName));
        Assert.That(result.ControllerName, Is.EqualTo(ControllerConstants.SearchOrganisationControllerName));
    }

    [Test]
    public async Task Then_Orchestrator_Create_Account_Is_Not_Called()
    {
        await _employerAccountController.Summary(_summaryViewModel);

        _orchestrator
            .Verify(
                m => m.CreateOrUpdateAccount(It.IsAny<CreateAccountModel>(), It.IsAny<HttpContext>())
                , Times.Never);
    }
}