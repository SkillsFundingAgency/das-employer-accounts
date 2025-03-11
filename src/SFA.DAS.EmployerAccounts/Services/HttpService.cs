using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Azure.Core;
using Azure.Identity;
using SFA.DAS.Api.Common.Interfaces;

namespace SFA.DAS.EmployerAccounts.Services;

[ExcludeFromCodeCoverage]
public class HttpService(string identifierUri, IAzureClientCredentialHelper azureClientCredentialHelper) : IHttpService
{
    public virtual Task<string> GetAsync(string url, bool exceptionOnNotFound = true)
    {
        return GetAsync(url, response => exceptionOnNotFound || response.StatusCode != HttpStatusCode.NotFound);
    }

    public virtual async Task<string> GetAsync(string url, Func<HttpResponseMessage, bool> responseChecker)
    {
        var accessToken = await azureClientCredentialHelper.GetAccessTokenAsync(identifierUri);

        using var client = new HttpClient();

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await client.SendAsync(requestMessage);

        if (responseChecker != null && !responseChecker(response))
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}