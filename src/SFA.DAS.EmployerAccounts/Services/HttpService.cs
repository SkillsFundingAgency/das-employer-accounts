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
    private readonly ChainedTokenCredential _azureServiceTokenProvider = new(
        new ManagedIdentityCredential(),
        new AzureCliCredential(),
        new VisualStudioCodeCredential(),
        new VisualStudioCredential()
    );

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