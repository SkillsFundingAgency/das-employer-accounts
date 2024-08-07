using System.Net.Http;
using Newtonsoft.Json;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Interfaces.OuterApi;

namespace SFA.DAS.EmployerAccounts.Infrastructure.OuterApi;

public class OuterApiClient(HttpClient httpClient, EmployerAccountsOuterApiConfiguration configuration) : IOuterApiClient
{
    public async Task<TResponse> Get<TResponse>(IGetApiRequest request)
    {
        using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, request.GetUrl);
        AddAuthenticationHeader(httpRequestMessage);

        using var response = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        
        return JsonConvert.DeserializeObject<TResponse>(json);
    }
    
    private void AddAuthenticationHeader(HttpRequestMessage httpRequestMessage)
    {
        httpRequestMessage.Headers.Add("Ocp-Apim-Subscription-Key", configuration.Key);
        httpRequestMessage.Headers.Add("X-Version", "1");
    }
}