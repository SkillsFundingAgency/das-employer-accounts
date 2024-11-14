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
        var cspValues = new StringValues(
                $"script-src 'self' 'unsafe-inline' 'unsafe-eval' {dasCdn} " +
                "https://das-prd-frnt-end.azureedge.net https://das-demo-frnt-end.azureedge.net https://das-pp-frnt-end.azureedge.net https://das-test-frnt-end.azureedge.net https://das-at-frnt-end.azureedge.net " +
                "*.googletagmanager.com *.google-analytics.com *.googleapis.com https://*.zdassets.com https://*.zendesk.com wss://*.zendesk.com wss://*.zopim.com; " +
                $"style-src 'self' 'unsafe-inline' {dasCdn} https://tagmanager.google.com https://fonts.googleapis.com https://*.rcrsv.io ; " +
                $"img-src {dasCdn} www.googletagmanager.com https://ssl.gstatic.com https://www.gstatic.com https://www.google-analytics.com https://*.test2-eas.apprenticeships.education.gov.uk ; " +
                $"font-src {dasCdn} https://fonts.gstatic.com https://*.rcrsv.io data: ;" +
                "connect-src 'self' https://*.google-analytics.com https://*.zendesk.com https://*.zdassets.com wss://*.zopim.com https://*.rcrsv.io ;");
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
