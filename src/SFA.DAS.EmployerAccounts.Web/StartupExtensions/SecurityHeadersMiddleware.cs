using Microsoft.Extensions.Primitives;

namespace SFA.DAS.EmployerAccounts.Web.StartupExtensions;

public class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        const string dasCdn = "das-at-frnt-end.azureedge.net das-pp-frnt-end.azureedge.net das-mo-frnt-end.azureedge.net das-test-frnt-end.azureedge.net das-test2-frnt-end.azureedge.net das-prd-frnt-end.azureedge.net";

        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("Content-Security-Policy", new StringValues(
                $"script-src 'self' 'unsafe-inline' 'unsafe-eval' {dasCdn} " +
                "https://das-prd-frnt-end.azureedge.net https://das-demo-frnt-end.azureedge.net https://das-pp-frnt-end.azureedge.net https://das-test-frnt-end.azureedge.net https://das-at-frnt-end.azureedge.net " +
                "*.googletagmanager.com *.google-analytics.com *.googleapis.com https://*.zdassets.com https://*.zendesk.com wss://*.zendesk.com wss://*.zopim.com; " +
                $"style-src 'self' 'unsafe-inline' {dasCdn} https://tagmanager.google.com https://fonts.googleapis.com https://*.rcrsv.io ; " +
                $"img-src {dasCdn} www.googletagmanager.com https://ssl.gstatic.com https://www.gstatic.com https://www.google-analytics.com https://*.test2-eas.apprenticeships.education.gov.uk ; " +
                $"font-src {dasCdn} https://fonts.gstatic.com https://*.rcrsv.io data: ;" +
                "connect-src 'self' https://*.google-analytics.com https://*.zendesk.com https://*.zdassets.com wss://*.zopim.com https://*.rcrsv.io ;"));
        context.Response.Headers.Append("X-Permitted-Cross-Domain-Policies", "none");

        await next(context);
    }
}