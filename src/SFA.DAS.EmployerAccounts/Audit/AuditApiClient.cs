﻿using System.Net.Http;
using System.Net.Http.Headers;
using SFA.DAS.Api.Common.Interfaces;
using SFA.DAS.EmployerAccounts.Audit.Types;

namespace SFA.DAS.EmployerAccounts.Audit
{
    public class AuditApiClient : IAuditApiClient
    {
        private readonly  HttpClient _httpClient;
        private readonly IAuditApiClientConfiguration _configuration;
        private readonly IAzureClientCredentialHelper _azureClientCredentialHelper;

        public AuditApiClient(
            HttpClient httpClient, 
            IAuditApiClientConfiguration configuration,
            IAzureClientCredentialHelper azureClientCredentialHelper)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _azureClientCredentialHelper = azureClientCredentialHelper;

            var baseUrl = _configuration.BaseUrl.EndsWith("/")
                ? _configuration.BaseUrl
                : _configuration.BaseUrl + "/"; 
            
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        public async Task Audit(AuditMessage request)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "");

            await AddAuthenticationHeader(httpRequestMessage);

            var response = await _httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
        }

        private async Task AddAuthenticationHeader(HttpRequestMessage httpRequestMessage)
        {
            if (!string.IsNullOrEmpty(_configuration.IdentifierUri))
            {
                var accessToken = await _azureClientCredentialHelper.GetAccessTokenAsync(_configuration.IdentifierUri);
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }
    }
}