using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using SFA.DAS.EmployerAccounts.Web.StartupExtensions;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.AppStart;

[TestFixture]
public class SecurityHeadersMiddlewareTests
{
    private Mock<RequestDelegate> _nextMock;
    private SecurityHeadersMiddleware _middleware;
    private const string dasCdn = "das-at-frnt-end.azureedge.net das-pp-frnt-end.azureedge.net das-mo-frnt-end.azureedge.net das-test-frnt-end.azureedge.net das-test2-frnt-end.azureedge.net das-prd-frnt-end.azureedge.net";
   
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
        var cspValues = new StringValues($"default-src *; script-src *; connect-src *; img-src *; style-src *; object-src *;");
        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.Headers.XFrameOptions.Should().BeEquivalentTo("DENY");
        context.Response.Headers.XContentTypeOptions.Should().BeEquivalentTo("nosniff");
        context.Response.Headers.ContentSecurityPolicy.Should().BeEquivalentTo(cspValues);
        context.Response.Headers.XFrameOptions.Should().BeEquivalentTo("DENY");
        context.Response.Headers["X-Permitted-Cross-Domain-Policies"].Should().BeEquivalentTo("none");

        _nextMock.Verify(next => next(context), Times.Once);
    }
}
