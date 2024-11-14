using FluentAssertions;
using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerAccounts.Web.StartupExtensions;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.AppStart;

[TestFixture]
public class SecurityHeadersMiddlewareTests
{
    private Mock<RequestDelegate> _nextMock;
    private SecurityHeadersMiddleware _middleware;

    [SetUp]
    public void Setup()
    {
        _nextMock = new Mock<RequestDelegate>();
        _middleware = new SecurityHeadersMiddleware(_nextMock.Object);
    }

    [Test]
    public async Task InvokeAsync_ShouldAddSecurityHeaders()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.XFrameOptions.Should().BeEquivalentTo("DENY");
        context.Response.Headers.XContentTypeOptions.Should().BeEquivalentTo("nosniff");
        context.Response.Headers.ContentSecurityPolicy.Should().BeEquivalentTo("default-src 'self';");
        context.Response.Headers.XFrameOptions.Should().BeEquivalentTo("DENY");
        context.Response.Headers["X-Permitted-Cross-Domain-Policies"].Should().BeEquivalentTo("none");

        _nextMock.Verify(next => next(context), Times.Once);
    }
}
