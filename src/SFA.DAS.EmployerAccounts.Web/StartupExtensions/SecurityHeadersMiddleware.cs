using Microsoft.Extensions.Primitives;

namespace SFA.DAS.EmployerAccounts.Web.StartupExtensions;

public class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Append("x-frame-options", new StringValues("DENY"));
        context.Response.Headers.Append("x-content-type-options", new StringValues("nosniff"));
        context.Response.Headers.Append("X-Permitted-Cross-Domain-Policies", new StringValues("none"));
        context.Response.Headers.Append("x-xss-protection", new StringValues("0"));
        context.Response.Headers.Append("Content-Security-Policy", new StringValues($"default-src *; script-src *; connect-src *; img-src *; style-src *; object-src *;"));

        await next(context);
    }
}