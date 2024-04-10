using System.Net.Http;
using Newtonsoft.Json;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Interfaces.OuterApi;

namespace SFA.DAS.EmployerAccounts.Infrastructure.OuterApi;

public class OuterApiClient : IOuterApiClient
{
    private readonly HttpClient _httpClient;
    private readonly EmployerAccountsOuterApiConfiguration _configuration;

    public OuterApiClient(HttpClient httpClient, EmployerAccountsOuterApiConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<TResponse> Get<TResponse>(IGetApiRequest request)
    {
        using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, request.GetUrl);
        AddAuthenticationHeader(httpRequestMessage);

        using var response = await _httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JsonConvert.DeserializeObject<TResponse>(json);
    }

    public async Task Post(IPostApiRequest request)
    {
        var stringContent = new StringContent(JsonConvert.SerializeObject(request.Data), System.Text.Encoding.UTF8, "application/json");
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, request.PostUrl)
        {
            Content = stringContent,
        };

        AddAuthenticationHeader(requestMessage);
        var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    private void AddAuthenticationHeader(HttpRequestMessage httpRequestMessage)
    {
        httpRequestMessage.Headers.Add("Ocp-Apim-Subscription-Key", _configuration.Key);
        httpRequestMessage.Headers.Add("X-Version", "1");
    }
}