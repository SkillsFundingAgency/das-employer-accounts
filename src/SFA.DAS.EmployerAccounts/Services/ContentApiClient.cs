using System.Net.Http;
using System.Net.Http.Headers;
using Azure.Core;
using Azure.Identity;
using SFA.DAS.Api.Common.Interfaces;

namespace SFA.DAS.EmployerAccounts.Services;

public class ContentApiClient : IContentApiClient
{
    private readonly string _apiBaseUrl;        
    private readonly string _identifierUri;
    private readonly HttpClient _client;
    private readonly IAzureClientCredentialHelper _azureClientCredentialHelper;

    public ContentApiClient(HttpClient client, IContentClientApiConfiguration configuration, IAzureClientCredentialHelper azureClientCredentialHelper)
    {
        _apiBaseUrl = configuration.ApiBaseUrl.EndsWith("/")
            ? configuration.ApiBaseUrl
            : configuration.ApiBaseUrl + "/";

        _identifierUri = configuration.IdentifierUri;
        _client = client;
        _azureClientCredentialHelper = azureClientCredentialHelper;
    }

    public async Task<string> Get(string type, string applicationId)
    {
        var uri = $"{_apiBaseUrl}api/content?applicationId={applicationId}&type={type}";
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
        await AddAuthenticationHeader(requestMessage);

        var response = await _client.SendAsync(requestMessage).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        return content;
    }

    private async Task AddAuthenticationHeader(HttpRequestMessage httpRequestMessage)
    {
        if (!string.IsNullOrEmpty(_identifierUri))
        {
            var azureServiceTokenProvider = new ChainedTokenCredential(
                new ManagedIdentityCredential(),
                new AzureCliCredential(),
                new VisualStudioCodeCredential(),
                new VisualStudioCredential()
            );
        
            var accessToken = (await azureServiceTokenProvider.GetTokenAsync(new TokenRequestContext(scopes: new string[] { _identifierUri }))).Token;
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
    }
}