using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using SFA.DAS.EmployerAccounts.Exceptions.Hmrc;
using SFA.DAS.EmployerAccounts.Interfaces.Hmrc;
using IHttpResponseLogger = SFA.DAS.EmployerAccounts.Interfaces.IHttpResponseLogger;

namespace SFA.DAS.EmployerAccounts.Infrastructure.Data;

public class HttpClientWrapper : IHttpClientWrapper
{
    private readonly IHttpResponseLogger _httpResponseLogger;

    public HttpClientWrapper(IHttpResponseLogger httpResponseLogger)
    {
        MediaTypeWithQualityHeaderValueList = new List<MediaTypeWithQualityHeaderValue>();
        _httpResponseLogger = httpResponseLogger;
    }

    public string AuthScheme { get; set; }
    public string BaseUrl { get; set; }
    public List<MediaTypeWithQualityHeaderValue> MediaTypeWithQualityHeaderValueList { get; set; }

    public async Task<string> SendMessage<T>(T content, string url)
    {
        using var httpClient = CreateHttpClient();

        var serializeObject = JsonConvert.SerializeObject(content);
        using var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(serializeObject, System.Text.Encoding.UTF8, "application/json")
        });
        await EnsureSuccessfulResponse(response);

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<T> Get<T>(string authToken, string url)
    {
        using var httpClient = CreateHttpClient();
        using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
        
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue(AuthScheme, authToken);
        
        using var response = await httpClient.SendAsync(httpRequestMessage);
        await EnsureSuccessfulResponse(response);

        return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
    }

    public async Task<string> GetString(string url, string accessToken)
    {
        using var client = new HttpClient();
        using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);

        if (!string.IsNullOrEmpty(accessToken))
        {
            var authScheme = !string.IsNullOrEmpty(AuthScheme)
                ? AuthScheme
                : "Bearer";

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue(authScheme, accessToken);
        }

        using var response = await client.SendAsync(httpRequest);
        await EnsureSuccessfulResponse(response);

        return await response.Content.ReadAsStringAsync();
    }

    private HttpClient CreateHttpClient()
    {
        if (string.IsNullOrEmpty(BaseUrl)) throw new ArgumentNullException(nameof(BaseUrl));

        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl)
        };

        if (!MediaTypeWithQualityHeaderValueList.Any())
        {
            return httpClient;
        }

        foreach (var mediaTypeWithQualityHeaderValue in MediaTypeWithQualityHeaderValueList)
        {
            httpClient.DefaultRequestHeaders.Accept.Add(mediaTypeWithQualityHeaderValue);
        }

        return httpClient;
    }

    private async Task EnsureSuccessfulResponse(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        switch ((int)response.StatusCode)
        {
            case 404:
                throw new ResourceNotFoundException(response.RequestMessage.RequestUri.ToString());
            case 408:
                throw new RequestTimeOutException();
            case 429:
                throw new TooManyRequestsException();
            case 500:
                throw new InternalServerErrorException();
            case 503:
                throw new ServiceUnavailableException();
            default:
                if ((int)response.StatusCode == 400) await _httpResponseLogger.LogResponseAsync(response);

                throw new HttpException((int)response.StatusCode, $"Unexpected HTTP exception - ({(int)response.StatusCode}): {response.ReasonPhrase}");
        }
    }
}