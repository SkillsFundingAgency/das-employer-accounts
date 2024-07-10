﻿using System;
using System.Threading.Tasks;
using HMRC.ESFA.Levy.Api.Client;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Caches;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Interfaces.Hmrc;
using SFA.DAS.EmployerAccounts.Services;
using SFA.DAS.EmployerAccounts.UnitTests.Policies.Hmrc;
using SFA.DAS.TokenService.Api.Client;
using SFA.DAS.TokenService.Api.Types;

namespace SFA.DAS.EmployerAccounts.UnitTests.Services.Hmrc;

internal class WhenICallHmrcServiceForLastEnglishFractionUpdateDate
{
    private const string ExpectedBaseUrl = "http://hmrcbase.gov.uk/auth/";
    private const string ExpectedClientId = "654321";
    private const string ExpectedScope = "emp_ref";
    private const string ExpectedClientSecret = "my_secret";
    private const string ExpectedAccessCode = "789654321AGFVD";
    private Mock<IApprenticeshipLevyApiClient> _apprenticeshipLevyApiClient;
    private Mock<IInProcessCache> _cacheProvider;
    private IHmrcConfiguration _configuration;

    private HmrcService _hmrcService;
    private Mock<IHttpClientWrapper> _httpClientWrapper;

    private Mock<ITokenServiceApiClient> _tokenService;

    [SetUp]
    public void Arrange()
    {
        _configuration = new HmrcConfiguration
        {
            BaseUrl = ExpectedBaseUrl,
            ClientId = ExpectedClientId,
            Scope = ExpectedScope,
            ClientSecret = ExpectedClientSecret,
            ServerToken = "token1234"
        };

        _httpClientWrapper = new Mock<IHttpClientWrapper>();
        _apprenticeshipLevyApiClient = new Mock<IApprenticeshipLevyApiClient>();

        _tokenService = new Mock<ITokenServiceApiClient>();
        _tokenService.Setup(x => x.GetPrivilegedAccessTokenAsync()).ReturnsAsync(new PrivilegedAccessToken { AccessCode = ExpectedAccessCode });

        _cacheProvider = new Mock<IInProcessCache>();
        _cacheProvider.SetupSequence(c => c.Get<DateTime?>("HmrcFractionLastCalculatedDate"))
            .Returns((DateTime?) null)
            .Returns(new DateTime());

        _hmrcService = new HmrcService(_configuration, _httpClientWrapper.Object,
            _apprenticeshipLevyApiClient.Object, _tokenService.Object, new NoopExecutionPolicy(),
            _cacheProvider.Object, null, new Mock<ILogger<HmrcService>>().Object);
    }

    [Test]
    public async Task ThenIShouldGetTheCurrentUpdatedDate()
    {
        //Assign
        var updateDate = DateTime.Now;

        _apprenticeshipLevyApiClient.Setup(x => x.GetLastEnglishFractionUpdate(It.IsAny<string>()))
            .ReturnsAsync(updateDate);

        //Act
        var result = await _hmrcService.GetLastEnglishFractionUpdate();

        //Assert
        _apprenticeshipLevyApiClient.Verify(x => x.GetLastEnglishFractionUpdate(ExpectedAccessCode), Times.Once);
        Assert.That(result, Is.EqualTo(updateDate));
    }


    [Test]
    public async Task ThenTheFractionLastCaclulatedDateIsReadFromTheCacheOnSubsequentReads()
    {
        //Act
        await _hmrcService.GetLastEnglishFractionUpdate();
        await _hmrcService.GetLastEnglishFractionUpdate();

        //Assert
        _apprenticeshipLevyApiClient.Verify(x => x.GetLastEnglishFractionUpdate(ExpectedAccessCode), Times.Once);
        _cacheProvider.Verify(x => x.Set("HmrcFractionLastCalculatedDate", It.IsAny<DateTime>(), It.Is<TimeSpan>(c => c.Minutes.Equals(30))));
    }
}