using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using SFA.DAS.EmployerAccounts.Queries.GetContent;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Helpers;

class HtmlHelpersTests
{
    private Mock<IMediator> _mockMediator;
    private Mock<ICompositeViewEngine> _mockCompositeViewEngine;
    private HtmlHelpers _htmlHelpers;

    [SetUp]
    public void SetUp()
    {
        _mockMediator = new Mock<IMediator>();
        _mockCompositeViewEngine = new Mock<ICompositeViewEngine>();
        _htmlHelpers = new HtmlHelpers(_mockMediator.Object, NullLogger<HtmlHelpers>.Instance, _mockCompositeViewEngine.Object);
    }

    [Test]
    public void GetContentByType_ReturnsContentFromMediator()
    {
        // Arrange
        const string contentType = "SomeType";
        _mockMediator
            .Setup(m => m.Send(It.Is<GetContentRequest>(r => r.ContentType == contentType), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetContentResponse { Content = "<p>content</p>" });

        // Act
        var actual = _htmlHelpers.GetContentByType(contentType);

        // Assert
        actual.Should().BeOfType<HtmlString>();
        actual.Value.Should().Be("<p>content</p>");
    }

    [Test]
    public void GetContentByType_WhenContentIsNull_ReturnsHtmlStringWithNullValue()
    {
        // Arrange
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetContentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetContentResponse { Content = null });

        // Act
        var actual = _htmlHelpers.GetContentByType("SomeType");

        // Assert
        actual.Value.Should().BeNull();
    }

    [Test]
    public async Task GetContentByTypeAsync_ReturnsContentFromMediator()
    {
        // Arrange
        const string contentType = "SomeType";
        _mockMediator
            .Setup(m => m.Send(It.Is<GetContentRequest>(r => r.ContentType == contentType), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetContentResponse { Content = "<p>async content</p>" });

        // Act
        var actual = await _htmlHelpers.GetContentByTypeAsync(contentType);

        // Assert
        actual.Should().BeOfType<HtmlString>();
        actual.Value.Should().Be("<p>async content</p>");
    }

    [Test]
    public void ViewExists_WhenViewFound_ReturnsTrue()
    {
        // Arrange
        var html = CreateHtmlHelper();
        var viewEngineResult = ViewEngineResult.Found("ViewName", new Mock<IView>().Object);
        _mockCompositeViewEngine
            .Setup(v => v.FindView(html.ViewContext, "ViewName", false))
            .Returns(viewEngineResult);

        // Act
        var actual = _htmlHelpers.ViewExists(html, "ViewName");

        // Assert
        actual.Should().BeTrue();
    }

    [Test]
    public void ViewExists_WhenViewNotFound_ReturnsFalse()
    {
        // Arrange
        var html = CreateHtmlHelper();
        var viewEngineResult = ViewEngineResult.NotFound("ViewName", Array.Empty<string>());
        _mockCompositeViewEngine
            .Setup(v => v.FindView(html.ViewContext, "ViewName", false))
            .Returns(viewEngineResult);

        // Act
        var actual = _htmlHelpers.ViewExists(html, "ViewName");

        // Assert
        actual.Should().BeFalse();
    }

    [TestCase("ABC123", "/accounts/ABC123/teams")]
    [TestCase(null, "/")]
    [TestCase("", "/")]
    public void ReturnToHomePageButtonHref_ReturnsExpectedHref(string hashedAccountId, string expected)
    {
        // Act
        var actual = _htmlHelpers.ReturnToHomePageButtonHref(hashedAccountId);

        // Assert
        actual.Should().Be(expected);
    }

    [TestCase("ABC123", "Go back to the account home page")]
    [TestCase(null, "Go back to the service home page")]
    [TestCase("", "Go back to the service home page")]
    public void ReturnToHomePageButtonText_ReturnsExpectedText(string hashedAccountId, string expected)
    {
        // Act
        var actual = _htmlHelpers.ReturnToHomePageButtonText(hashedAccountId);

        // Assert
        actual.Should().Be(expected);
    }

    [TestCase("ABC123", "Back to the homepage")]
    [TestCase(null, "Back")]
    [TestCase("", "Back")]
    public void ReturnToHomePageLinkText_ReturnsExpectedText(string hashedAccountId, string expected)
    {
        // Act
        var actual = _htmlHelpers.ReturnToHomePageLinkText(hashedAccountId);

        // Assert
        actual.Should().Be(expected);
    }

    private static IHtmlHelper CreateHtmlHelper()
    {
        var viewContext = new ViewContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
            new Mock<IView>().Object,
            new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()),
            new TempDataDictionary(new DefaultHttpContext(), new Mock<ITempDataProvider>().Object),
            TextWriter.Null,
            new HtmlHelperOptions());

        var mockHtmlHelper = new Mock<IHtmlHelper>();
        mockHtmlHelper.SetupGet(h => h.ViewContext).Returns(viewContext);
        return mockHtmlHelper.Object;
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