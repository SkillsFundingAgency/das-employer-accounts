using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Azure.Core;
using Azure.Identity;

namespace SFA.DAS.EmployerAccounts.Services;

[ExcludeFromCodeCoverage]
public class HttpService(string identifierUri) : IHttpService
{
    private const int MaxRetries = 2;
    private static readonly TimeSpan NetworkTimeout = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(100);
    
    private readonly ChainedTokenCredential _azureServiceTokenProvider = new ChainedTokenCredential(
        new ManagedIdentityCredential(options: new TokenCredentialOptions
        {
            Retry = { NetworkTimeout = NetworkTimeout, MaxRetries = MaxRetries, Delay = Delay, Mode = RetryMode.Fixed }
        }),
        new AzureCliCredential(options: new AzureCliCredentialOptions
        {
            Retry = { NetworkTimeout = NetworkTimeout, MaxRetries = MaxRetries, Delay = Delay, Mode = RetryMode.Fixed }
        }),
        new VisualStudioCredential(options: new VisualStudioCredentialOptions
        {
            Retry = { NetworkTimeout = NetworkTimeout, MaxRetries = MaxRetries, Delay = Delay, Mode = RetryMode.Fixed }
        }),
        new VisualStudioCodeCredential(options: new VisualStudioCodeCredentialOptions
        {
            Retry = { NetworkTimeout = NetworkTimeout, MaxRetries = MaxRetries, Delay = Delay, Mode = RetryMode.Fixed }
        }));

    public virtual Task<string> GetAsync(string url, bool exceptionOnNotFound = true)
    {
        return GetAsync(url, response => exceptionOnNotFound || response.StatusCode != HttpStatusCode.NotFound);
    }

    public virtual async Task<string> GetAsync(string url, Func<HttpResponseMessage, bool> responseChecker)
    {
        var accessToken = await GenerateAccessToken();

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await client.GetAsync(url);

        if (responseChecker != null && !responseChecker(response))
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
    
    private async Task<string> GenerateAccessToken()
    {
        return (await _azureServiceTokenProvider.GetTokenAsync(new TokenRequestContext(scopes: [identifierUri]))).Token;
    }
}