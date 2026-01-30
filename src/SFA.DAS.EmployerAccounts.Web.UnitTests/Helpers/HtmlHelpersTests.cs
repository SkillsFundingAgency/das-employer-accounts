using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Claim = System.Security.Claims.Claim;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Helpers;

class HtmlHelpersTests
{
    private Mock<HttpContext> _mockHttpContext;
    private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private Mock<ClaimsPrincipal> _mockPrincipal;
    private Mock<ClaimsIdentity> _mockClaimsIdentity;
    private readonly bool _isAuthenticated = true;
    private List<Claim> _claims;
    private string _userId;
    private Mock<IMediator> _mockMediator;
    private EmployerAccountsConfiguration _employerConfiguration;

    [SetUp]
    public void SetUp()
    {
        _mockHttpContext = new Mock<HttpContext>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockPrincipal = new Mock<ClaimsPrincipal>();
        _mockClaimsIdentity = new Mock<ClaimsIdentity>();
        _employerConfiguration = new EmployerAccountsConfiguration();
        _userId = "TestUser";

        _claims = [new(ControllerConstants.UserRefClaimKeyName, _userId)];

        _mockPrincipal.Setup(m => m.Identity).Returns(_mockClaimsIdentity.Object);
        _mockClaimsIdentity.Setup(m => m.IsAuthenticated).Returns(_isAuthenticated);
        _mockClaimsIdentity.Setup(m => m.Claims).Returns(_claims);
        _mockHttpContext.Setup(m => m.User).Returns(_mockPrincipal.Object);
        _mockHttpContextAccessor.Setup(x=> x.HttpContext).Returns(_mockHttpContext.Object);

        _mockMediator = new Mock<IMediator>();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(provider => provider.GetService(typeof(EmployerAccountsConfiguration)))
            .Returns(_employerConfiguration);
    }

    [TestCaseSource(nameof(LabelCases))]
    public void WhenICallSetZenDeskLabelsWithLabels_ThenTheKeywordsAreCorrect(string[] labels, string keywords)
    {
        // Arrange
        var expected = $"<script type=\"text/javascript\">zE('webWidget', 'helpCenter:setSuggestions', {{ labels: [{keywords}] }});</script>";

        // Act
        var actual = HtmlHelpers.SetZenDeskLabels(labels).ToString();

        // Assert
        actual.Should().Be(expected);
    }

    private static readonly object[] LabelCases =
    [
        new object[] { new[] { "a string with multiple words", "the title of another page" }, "'a string with multiple words','the title of another page'"},
        new object[] { new[] { "eas-estimate-apprenticeships-you-could-fund" }, "'eas-estimate-apprenticeships-you-could-fund'"},
        new object[] { new[] { "eas-apostrophe's" }, @"'eas-apostrophe\'s'"},
        new object[] { new string[] { null }, "''" }
    ];
}