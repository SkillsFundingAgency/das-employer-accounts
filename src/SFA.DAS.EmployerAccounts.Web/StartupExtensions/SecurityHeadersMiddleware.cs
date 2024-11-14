namespace SFA.DAS.EmployerAccounts.Web.StartupExtensions;

public class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("Content-Security-Policy", "default-src 'self';");
        context.Response.Headers.Append("X-Permitted-Cross-Domain-Policies", "none");

        await next(context);
    }
}