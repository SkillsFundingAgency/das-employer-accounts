using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Claim = System.Security.Claims.Claim;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Helpers;

class UrlActionHelperTests
{
    private Mock<HttpContext> _mockHttpContext;
    private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private Mock<IConfiguration> _config;
    private IUrlActionHelper _urlActionHelper;
    private EmployerAccountsConfiguration _employerAccountsConfiguration;
    private readonly string _hashedAccountId = "ABCDE";
    private readonly string _environmentName = "at";

    [SetUp]
    public void SetUp()
    {
        _mockHttpContext = new Mock<HttpContext>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _employerAccountsConfiguration = new EmployerAccountsConfiguration();
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);

        _config = new Mock<IConfiguration>();
        _config.Setup(x => x["ResourceEnvironmentName"]).Returns(_environmentName);

        _urlActionHelper = new UrlActionHelper(_employerAccountsConfiguration, _mockHttpContextAccessor.Object,
            _config.Object);
    }

    [Test]
    public void WhenGettingRATDashboard_ShouldReturnCorrectUrl()
    {
        //Arrange
        var routeValues = new RouteValueDictionary
        {
            { ControllerConstants.AccountHashedIdRouteKeyName, _hashedAccountId }
        };
        _mockHttpContext.Setup(c => c.Request.RouteValues).Returns(routeValues);

        //Act
        var employerRATDashboardUrl = _urlActionHelper.EmployerRequestApprenticeshipTrainingAction("Dashboard");

        //Assert
        var dashboardPath = $"https://requesttraining.{_environmentName}-eas.apprenticeships.education.gov.uk/accounts/{_hashedAccountId}/employerrequests/dashboard";
        var expectedDashboardUrl = string.Format("{0}", dashboardPath);
        employerRATDashboardUrl.Should().Be(expectedDashboardUrl);
    }
}