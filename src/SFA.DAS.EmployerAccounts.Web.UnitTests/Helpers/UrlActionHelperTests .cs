using System.Security.Claims;
using Aspose.Pdf.Forms;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Claim = System.Security.Claims.Claim;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Helpers;

class UrlActionHelperTests
{
    private Mock<HttpContext> _mockHttpContext;
    private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private Mock<ClaimsPrincipal> _mockPrincipal;
    private Mock<ClaimsIdentity> _mockClaimsIdentity;
    private bool _isAuthenticated = true;
    private List<Claim> _claims;
    private string _userId;
    private IUrlActionHelper _urlActionHelper;
    private EmployerAccountsConfiguration _employerAccountsConfiguration;
    private readonly string _hashedAccountId = "ABCDE";

    [SetUp]
    public void SetUp()
    {
        _mockHttpContext = new Mock<HttpContext>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _employerAccountsConfiguration = new EmployerAccountsConfiguration()
        {
            EmployerRequestApprenticeshipTrainingBaseUrl = @"http://requesttraining.therestoftheurl.com/",
        };
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);

        _urlActionHelper = new UrlActionHelper(_employerAccountsConfiguration, _mockHttpContextAccessor.Object,
            new Mock<IConfiguration>().Object);
    }

    [Test]
    public void WhenGettingRATUrlWithoutHashedId_ShouldReturnCorrectUrl()
    {
        //Arrange
        var routeValues = new RouteValueDictionary
        {
            { ControllerConstants.AccountHashedIdRouteKeyName, null }
        };
        _mockHttpContext.Setup(c => c.Request.RouteValues).Returns(routeValues);

        //Act
        var employerRATDashboardUrl = _urlActionHelper.EmployerRequestApprenticeshipTrainingAction("dashboard");

        //Assert
        var expectedDashboardUrl = string.Format("{0}dashboard",_employerAccountsConfiguration.EmployerRequestApprenticeshipTrainingBaseUrl);
        employerRATDashboardUrl.Should().Be(expectedDashboardUrl);
    }

    [Test]
    public void WhenGettingRATUrlWithHashedId_ShouldReturnCorrectUrl()
    {
        //Arrange
        var routeValues = new RouteValueDictionary
        {
            { ControllerConstants.AccountHashedIdRouteKeyName, _hashedAccountId }
        };
        _mockHttpContext.Setup(c => c.Request.RouteValues).Returns(routeValues);

        //Act
        var employerRATDashboardUrl = _urlActionHelper.EmployerRequestApprenticeshipTrainingAction("dashboard");

        //Assert
        var expectedDashboardUrl = string.Format("{0}{1}/dashboard", 
            _employerAccountsConfiguration.EmployerRequestApprenticeshipTrainingBaseUrl, _hashedAccountId);
        employerRATDashboardUrl.Should().Be(expectedDashboardUrl);
    }
}