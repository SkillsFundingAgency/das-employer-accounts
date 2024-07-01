using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerAccountControllerTests.Summary.CreateAccount.Given_Account_Data_Is_Not_Null.AndReturnUrlIsSpecified;

public class WhenICreateAnAccount : ControllerTestBase
{
    private EmployerAccountController _employerAccountController;
    private Mock<EmployerAccountOrchestrator> _orchestrator;
    private const string ExpectedRedirectUrl = "http://redirect.local.test";
    private EmployerAccountData _accountData;
    private OrchestratorResponse<EmployerAgreementViewModel> _response;
    private Mock<ICookieStorageService<FlashMessageViewModel>> _flashMessage;
    private Mock<ICookieStorageService<ReturnUrlModel>> _returnUrlCookieStorage;
    private const string HashedAccountId = "ABC123";
    private const string ExpectedReturnUrl = "test.com";
    private SummaryViewModel _summaryViewModel;

    [SetUp]
    public void Arrange()
    {
        base.Arrange(ExpectedRedirectUrl);

        _summaryViewModel = new SummaryViewModel { IsOrganisationWithCorrectAddress = true };

        _orchestrator = new Mock<EmployerAccountOrchestrator>();

        var logger = new Mock<ILogger<EmployerAccountController>>();
        _flashMessage = new Mock<ICookieStorageService<FlashMessageViewModel>>();
        _returnUrlCookieStorage = new Mock<ICookieStorageService<ReturnUrlModel>>();

        _accountData = new EmployerAccountData
        {
            EmployerAccountOrganisationData = new EmployerAccountOrganisationData
            {
                OrganisationName = "Test Corp",
                OrganisationReferenceNumber = "1244454",
                OrganisationRegisteredAddress = "1, Test Street",
                OrganisationDateOfInception = DateTime.Now.AddYears(-10),
                OrganisationStatus = "active",
                OrganisationType = OrganisationType.Charities,
                Sector = "Public"
            },
            EmployerAccountPayeRefData = new EmployerAccountPayeRefData
            {
                PayeReference = "123/ABC",
                EmployerRefName = "Scheme 1",
                RefreshToken = "123",
                AccessToken = "456",
                EmpRefNotFound = true,
            }
        };

        AddUserToContext();

        _orchestrator.Setup(x => x.GetCookieData())
            .Returns(_accountData);

        _response = new OrchestratorResponse<EmployerAgreementViewModel>()
        {
            Data = new EmployerAgreementViewModel
            {
                EmployerAgreement = new EmployerAgreementView
                {
                    HashedAccountId = HashedAccountId
                }
            },
            Status = HttpStatusCode.OK
        };

        _orchestrator.Setup(x => x.CreateOrUpdateAccount(It.IsAny<CreateAccountModel>(), It.IsAny<HttpContext>()))
            .ReturnsAsync(_response);

        _returnUrlCookieStorage.Setup(x => x.Get("SFA.DAS.EmployerAccounts.Web.Controllers.ReturnUrlCookie"))
            .Returns(new ReturnUrlModel { Value = ExpectedReturnUrl });

        _employerAccountController = new EmployerAccountController(
            _orchestrator.Object,
            logger.Object,
            _flashMessage.Object,
            Mock.Of<IMediator>(),
            _returnUrlCookieStorage.Object,
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
    public async Task ThenIShouldGoToTheReturnUrl()
    {
        //Act
        var result = await _employerAccountController.Summary(_summaryViewModel) as RedirectResult;

        //Assert
        Assert.That(result.Url, Is.EqualTo(ExpectedReturnUrl));
    }
}