using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmployerAccounts.Infrastructure.Data;

public class HttpResponseLogger(ILogger<HttpResponseLogger> logger) : IHttpResponseLogger
{
    public async Task LogResponseAsync(HttpResponseMessage response)
    {
        if (IsContentStringType(response))
        {
            var content = await response.Content.ReadAsStringAsync();

            logger.LogDebug("Logged response {Response}", new Dictionary<string, object>
            {
                { "StatusCode", response.StatusCode },
                { "Reason", response.ReasonPhrase },
                { "Content", content }
            });
        }
    }

    private static bool IsContentStringType(HttpResponseMessage response)
    {
        return response?.Content?.Headers?.ContentType != null && (
            response.Content.Headers.ContentType.MediaType.StartsWith("text") ||
            response.Content.Headers.ContentType.MediaType == "application/json");
    }
}