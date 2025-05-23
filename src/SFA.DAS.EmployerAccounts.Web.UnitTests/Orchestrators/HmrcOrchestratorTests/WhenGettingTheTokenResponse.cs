﻿using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Queries.GetGatewayToken;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Orchestrators.HmrcOrchestratorTests;

public class WhenGettingTheTokenResponse
{
    private EmployerAccountOrchestrator _employerAccountOrchestrator;
    private Mock<ILogger<EmployerAccountOrchestrator>> _logger;
    private Mock<IMediator> _mediator;
    private Mock<ICookieStorageService<EmployerAccountData>> _cookieService;
    private EmployerAccountsConfiguration _configuration;
    private Mock<IEmployerAccountService> _employerAccountService;

    [SetUp]
    public void Arrange()
    {
        _logger = new Mock<ILogger<EmployerAccountOrchestrator>>();
        _mediator = new Mock<IMediator>();
        _cookieService = new Mock<ICookieStorageService<EmployerAccountData>>();
        _employerAccountService = new Mock<IEmployerAccountService>();
        _configuration = new EmployerAccountsConfiguration
        {
            Hmrc = new HmrcConfiguration()
        };

        _employerAccountOrchestrator = new EmployerAccountOrchestrator(_mediator.Object, 
            _logger.Object, 
            _cookieService.Object, 
            _configuration, 
            Mock.Of<IEncodingService>(),
            Mock.Of<IUrlActionHelper>());
    }

    [Test]
    public async Task ThenTheTokenIsRetrievedFromTheQuery()
    {
        //Arrange
        var accessCode = "546tg";
        var returnUrl = "http://someUrl";
        _mediator.Setup(x => x.Send(It.IsAny<GetGatewayTokenQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetGatewayTokenQueryResponse { HmrcTokenResponse = new HmrcTokenResponse() });

        //Act
        var token = await _employerAccountOrchestrator.GetGatewayTokenResponse(accessCode, returnUrl, null);

        //Assert
        _mediator.Verify(x => x.Send(It.Is<GetGatewayTokenQuery>(c => c.AccessCode.Equals(accessCode) && c.RedirectUrl.Equals(returnUrl)), It.IsAny<CancellationToken>()));
        Assert.That(token.Data, Is.AssignableFrom<HmrcTokenResponse>());
    }

    [Test]
    public async Task ThenTheFlashMessageIsPopulatedWhenAuthorityIsNotGranted()
    {
        //Arrange
        var store = new Dictionary<string, StringValues>
        {
            { "error", new StringValues("USER_DENIED_AUTHORIZATION") },
            { "error_Code", new StringValues("USER_DENIED_AUTHORIZATION") }
        };

        var queryCollection = new QueryCollection(store);

        //Act
        var actual = await _employerAccountOrchestrator.GetGatewayTokenResponse(string.Empty, string.Empty, queryCollection);

        //Assert
        Assert.That(actual, Is.AssignableFrom<OrchestratorResponse<HmrcTokenResponse>>());
        Assert.That(actual.FlashMessage.Headline, Is.EqualTo("Account not added"));
        Assert.That(actual.FlashMessage.SeverityCssClass, Is.EqualTo("error-summary"));
        Assert.That(actual.FlashMessage.Severity, Is.EqualTo(FlashMessageSeverityLevel.Error));
        Assert.That(actual.FlashMessage.ErrorMessages, Does.Contain(new KeyValuePair<string, string>("agree_and_continue", "Agree and continue")));
    }
}