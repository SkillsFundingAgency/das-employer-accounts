using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Azure.Core;
using Azure.Identity;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace SFA.DAS.EmployerAccounts.Services;

public class HttpService(string clientId, string clientSecret, string identifierUri, string tenant)
    : IHttpService
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
        var accessToken = await GetAccessToken();

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

    private async Task<string> GetAccessToken()
    {
        var accessToken = IsClientCredentialConfiguration(clientId, clientSecret, tenant)
            ? await GetClientCredentialAuthenticationResult(clientId, clientSecret, identifierUri, tenant)
            : await GetManagedIdentityAuthenticationResult(identifierUri);

        return accessToken;
    }

    private static async Task<string> GetClientCredentialAuthenticationResult(string clientId, string clientSecret, string resource, string tenant)
    {
        var authority = $"https://login.microsoftonline.com/{tenant}";
        var clientCredential = new ClientCredential(clientId, clientSecret);
        var context = new AuthenticationContext(authority, true);
        var result = await context.AcquireTokenAsync(resource, clientCredential);
        return result.AccessToken;
    }

    private async Task<string> GetManagedIdentityAuthenticationResult(string resource)
    {
        return (await _azureServiceTokenProvider.GetTokenAsync(new TokenRequestContext(scopes: [resource]))).Token;
    }

    private static bool IsClientCredentialConfiguration(string clientId, string clientSecret, string tenant)
    {
        return !string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret) && !string.IsNullOrEmpty(tenant);
    }
}