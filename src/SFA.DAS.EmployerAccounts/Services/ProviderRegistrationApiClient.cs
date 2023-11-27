using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using SFA.DAS.Authentication.Extensions.Legacy;

namespace SFA.DAS.EmployerAccounts.Services;

public class ProviderRegistrationApiClient : ApiClientBase, IProviderRegistrationApiClient
{
    private readonly string _apiBaseUrl;
    private readonly string _identifierUri;
    private readonly HttpClient _client;
    private readonly ILogger<ProviderRegistrationApiClient> _logger;

    public ProviderRegistrationApiClient(HttpClient client,
        IProviderRegistrationClientApiConfiguration configuration,
        ILogger<ProviderRegistrationApiClient> logger
    ) : base(client)
    {
        _apiBaseUrl = configuration.BaseUrl.EndsWith("/")
            ? configuration.BaseUrl
            : configuration.BaseUrl + "/";

        _identifierUri = configuration.IdentifierUri;
        _client = client;
        _logger = logger;
    }

    public async Task Unsubscribe(string correlationId)
    {
        var url = $"{_apiBaseUrl}api/unsubscribe/{correlationId}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        await AddAuthenticationHeaders(request);

        _logger.LogInformation("Getting Unsubscribe {Url}", url);

        await _client.SendAsync(request);
    }

    public async Task<string> GetInvitations(string correlationId)
    {
        var url = $"{_apiBaseUrl}api/invitations/{correlationId}";
        _logger.LogInformation("Getting Invitations {Url}", url);

        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        using var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }

    private async Task AddAuthenticationHeaders(HttpRequestMessage httpRequestMessage)
    {
        if (!string.IsNullOrEmpty(_identifierUri))
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var accessToken = await azureServiceTokenProvider.GetAccessTokenAsync(_identifierUri);
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
    }
}