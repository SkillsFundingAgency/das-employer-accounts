using MediatR;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerAccounts.Queries.GetContent;
using SFA.DAS.EmployerAccounts.Web.ViewComponents;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.ViewComponents;

[TestFixture]
public class ContentViewComponentTests
{
    private Mock<IMediator> _mockMediator;
    private Mock<ILogger<ContentViewComponent>> _mockLogger;
    private ContentViewComponent _contentViewComponent;

    [SetUp]
    public void SetUp()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<ContentViewComponent>>();
        _contentViewComponent = new ContentViewComponent(_mockMediator.Object, _mockLogger.Object);
    }

    [Test]
    public async Task InvokeAsync_WhenTypeIsProvided_ThenReturnsContentResult()
    {
        // Arrange
        var contentType = "banner";
        var expectedContent = "<div>Test Content</div>";
        var response = new GetContentResponse
        {
            Content = expectedContent,
            HasFailed = false
        };

        _mockMediator
            .Setup(x => x.Send(It.Is<GetContentRequest>(r => r.ContentType == contentType), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _contentViewComponent.InvokeAsync(contentType);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ContentViewComponentResult>();
        var contentResult = result as ContentViewComponentResult;
        contentResult.Content.Should().Be(expectedContent);
    }

    [Test]
    public async Task InvokeAsync_WhenResponseHasFailed_ThenReturnsEmptyContent()
    {
        // Arrange
        var contentType = "banner";
        var response = new GetContentResponse
        {
            Content = null,
            HasFailed = true
        };

        _mockMediator
            .Setup(x => x.Send(It.Is<GetContentRequest>(r => r.ContentType == contentType), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _contentViewComponent.InvokeAsync(contentType);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ContentViewComponentResult>();
        var contentResult = result as ContentViewComponentResult;
        contentResult.Content.Should().Be(string.Empty);
    }

    [Test]
    public async Task InvokeAsync_WhenContentIsNull_ThenReturnsEmptyContent()
    {
        // Arrange
        var contentType = "banner";
        var response = new GetContentResponse
        {
            Content = null,
            HasFailed = false
        };

        _mockMediator
            .Setup(x => x.Send(It.Is<GetContentRequest>(r => r.ContentType == contentType), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _contentViewComponent.InvokeAsync(contentType);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ContentViewComponentResult>();
        var contentResult = result as ContentViewComponentResult;
        contentResult.Content.Should().Be(string.Empty);
    }

    [Test]
    public async Task InvokeAsync_WhenTypeIsNull_ThenReturnsEmptyContent()
    {
        // Act
        var result = await _contentViewComponent.InvokeAsync(null);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ContentViewComponentResult>();
        var contentResult = result as ContentViewComponentResult;
        contentResult.Content.Should().Be(string.Empty);
        _mockMediator.Verify(x => x.Send(It.IsAny<GetContentRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task InvokeAsync_WhenTypeIsEmpty_ThenReturnsEmptyContent()
    {
        // Act
        var result = await _contentViewComponent.InvokeAsync(string.Empty);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ContentViewComponentResult>();
        var contentResult = result as ContentViewComponentResult;
        contentResult.Content.Should().Be(string.Empty);
        _mockMediator.Verify(x => x.Send(It.IsAny<GetContentRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task InvokeAsync_WhenTypeIsWhitespace_ThenReturnsEmptyContent()
    {
        // Act
        var result = await _contentViewComponent.InvokeAsync("   ");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ContentViewComponentResult>();
        var contentResult = result as ContentViewComponentResult;
        contentResult.Content.Should().Be(string.Empty);
        _mockMediator.Verify(x => x.Send(It.IsAny<GetContentRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task InvokeAsync_WhenExceptionIsThrown_ThenReturnsEmptyContent()
    {
        // Arrange
        var contentType = "banner";
        _mockMediator
            .Setup(x => x.Send(It.Is<GetContentRequest>(r => r.ContentType == contentType), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _contentViewComponent.InvokeAsync(contentType);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ContentViewComponentResult>();
        var contentResult = result as ContentViewComponentResult;
        contentResult.Content.Should().Be(string.Empty);
    }
}

