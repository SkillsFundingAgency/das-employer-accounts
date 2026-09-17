using System.IO;
using System.Text.RegularExpressions;

namespace SFA.DAS.EmployerAccounts.Web.StartupExtensions;

/// <summary>
/// Isolation-only: Shared UI emits absolute accounts.{env}-eas... links. Rewrite them to the
/// current request origin so Home/menu stay on localhost or an ngrok tunnel.
/// </summary>
public class IsolationExternalLinkRewriteMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private static readonly Regex EmployerAccountsHostRegex = new(
        @"https://accounts\.(?:(?:local|test|test2|at|pp|demo|prd)-eas\.apprenticeships\.education\.gov\.uk|manage-apprenticeships\.service\.gov\.uk)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public async Task InvokeAsync(HttpContext context)
    {
        var isolation = string.Equals(configuration["IsolationMode"], "true", StringComparison.OrdinalIgnoreCase);
        if (!isolation)
        {
            await next(context);
            return;
        }

        var originalBody = context.Response.Body;
        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await next(context);

            var contentType = context.Response.ContentType ?? string.Empty;
            if (context.Response.StatusCode == 200
                && contentType.Contains("text/html", StringComparison.OrdinalIgnoreCase))
            {
                buffer.Position = 0;
                using var reader = new StreamReader(buffer, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
                var html = await reader.ReadToEndAsync();

                var origin = $"{context.Request.Scheme}://{context.Request.Host}".TrimEnd('/');
                var rewritten = EmployerAccountsHostRegex.Replace(html, origin);

                var bytes = System.Text.Encoding.UTF8.GetBytes(rewritten);
                context.Response.ContentLength = bytes.Length;
                context.Response.Body = originalBody;
                await context.Response.Body.WriteAsync(bytes);
            }
            else
            {
                buffer.Position = 0;
                context.Response.Body = originalBody;
                await buffer.CopyToAsync(context.Response.Body);
            }
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }
}
