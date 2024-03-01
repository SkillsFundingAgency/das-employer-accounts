using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.UserAccounts;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.UnitTests.Infrastructure
{
    public class WhenHandlingAPostRequest
    {
        [Test, MoqAutoData]
        public async Task Then_The_Endpoint_Is_Called_With_Authentication_Header_And_Data_Posted(AddProviderDetailsPostRequest postTestRequest)
        {
            // Arrange
            const string key = "123-abc-567";

            var config = new EmployerAccountsOuterApiConfiguration { BaseUrl = "http://valid-url/", Key = key };

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created
            };

            var httpMessageHandler = SetupMessageHandlerMock(response, $"{config.BaseUrl}{postTestRequest.PostUrl}", config.Key);
            var httpClient = new HttpClient(httpMessageHandler.Object) { BaseAddress = new Uri(config.BaseUrl) };
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", config.Key);
            httpClient.DefaultRequestHeaders.Add("X-Version", "1");

            var apiClient = new OuterApiClient(httpClient, config);

            // Act
            await apiClient.Post(postTestRequest);

            // Assert
            httpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(c =>
                    c.Method.Equals(HttpMethod.Post)
                    && c.Headers.Contains("Ocp-Apim-Subscription-Key")
                    && c.Headers.GetValues("Ocp-Apim-Subscription-Key").Single().Equals(key)
                    && c.Headers.Contains("X-Version")
                    && c.Headers.GetValues("X-Version").Single().Equals("1")
                    && c.RequestUri.AbsoluteUri.Equals($"{config.BaseUrl}{postTestRequest.PostUrl}")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Test, MoqAutoData]
        public async Task Then_If_It_Is_Not_Successful_An_Exception_Is_ThrownAsync(AddProviderDetailsPostRequest postTestRequest)
        {
            // Arrange
            const string key = "123-abc-567";

            var config = new EmployerAccountsOuterApiConfiguration { BaseUrl = "http://valid-url/", Key = key };

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            };

            var httpMessageHandler = SetupMessageHandlerMock(response, $"{config.BaseUrl}{postTestRequest.PostUrl}", config.Key);
            var httpClient = new HttpClient(httpMessageHandler.Object) { BaseAddress = new Uri(config.BaseUrl) };

            var apiClient = new OuterApiClient(httpClient, config);

            await apiClient.Invoking(async x => await x.Post(postTestRequest)).Should().ThrowAsync<HttpRequestException>();
        }

        private static Mock<HttpMessageHandler> SetupMessageHandlerMock(HttpResponseMessage response, string url, string key)
        {
            var httpMessageHandler = new Mock<HttpMessageHandler>();

            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(c =>
                        c.Method.Equals(HttpMethod.Post)
                        && c.Headers.Contains("Ocp-Apim-Subscription-Key")
                        && c.Headers.GetValues("Ocp-Apim-Subscription-Key").Single().Equals(key)
                        && c.Headers.Contains("X-Version")
                        && c.Headers.GetValues("X-Version").Single().Equals("1")
                        && c.RequestUri.AbsoluteUri.Equals(url)),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            return httpMessageHandler;
        }
    }
}
