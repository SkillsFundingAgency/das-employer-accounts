﻿using System.Net.Http;
using System.Net.Http.Headers;
using SFA.DAS.EmployerAccounts.Infrastructure.AzureTokenService;

namespace SFA.DAS.EmployerAccounts.Services;

public class ContentApiClient : IContentApiClient
{
    private readonly string _apiBaseUrl;        
    private readonly string _identifierUri;
    private readonly HttpClient _client;
    private readonly IAzureServiceTokenProvider _azureServiceTokenProvider;

    public ContentApiClient(
        HttpClient client, 
        IContentClientApiConfiguration configuration, 
        IAzureServiceTokenProvider azureServiceTokenProvider)
    {
        _apiBaseUrl = configuration.ApiBaseUrl.EndsWith("/")
            ? configuration.ApiBaseUrl
            : configuration.ApiBaseUrl + "/";

        _identifierUri = configuration.IdentifierUri;
        _client = client;
        _azureServiceTokenProvider = azureServiceTokenProvider;
    }

    public async Task<string> Get(string type, string applicationId)
    {
        var uri = $"{_apiBaseUrl}api/content?applicationId={applicationId}&type={type}";
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
        await AddAuthenticationHeader(requestMessage);

        using var response = await _client.SendAsync(requestMessage).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        return content;
    }

    private async Task AddAuthenticationHeader(HttpRequestMessage httpRequestMessage)
    {
        if (!string.IsNullOrEmpty(_identifierUri))
        {
            var accessToken = await _azureServiceTokenProvider.GetTokenAsync(_identifierUri);
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
    }
}