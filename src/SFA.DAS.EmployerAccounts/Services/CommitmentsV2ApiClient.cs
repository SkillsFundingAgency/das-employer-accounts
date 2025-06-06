﻿using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Headers;
using Azure.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Api.Common.Interfaces;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Extensions;

namespace SFA.DAS.EmployerAccounts.Services;

[ExcludeFromCodeCoverage]
public class CommitmentsV2ApiClient(
    HttpClient httpClient,
    CommitmentsApiV2ClientConfiguration config,
    ILogger<CommitmentsV2ApiClient> logger,
    IAzureClientCredentialHelper azureCredentialHelper)
    : ICommitmentsV2ApiClient
{
    public async Task<GetApprenticeshipResponse> GetApprenticeship(long apprenticeshipId)
    {
        var url = $"{BaseUrl()}api/apprenticeships/{apprenticeshipId}";

        logger.LogInformation("Getting GetApprenticeship {Url}", url);

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
        await AddAuthenticationHeader(requestMessage);

        using var response = await httpClient.SendAsync(requestMessage).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        return JsonConvert.DeserializeObject<GetApprenticeshipResponse>(json);
    }

    public async Task<GetApprenticeshipsResponse> GetApprenticeships(GetApprenticeshipsRequest request)
    {
        var url = $"{BaseUrl()}api/apprenticeships/?accountId={request.AccountId}&reverseSort={request.ReverseSort}{request.SortField}{request.SortField}{request.SearchTerm}";

        logger.LogInformation("Getting GetApprenticeships {Url}", url);

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
        await AddAuthenticationHeader(requestMessage);

        using var response = await httpClient.SendAsync(requestMessage).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JsonConvert.DeserializeObject<GetApprenticeshipsResponse>(json);
    }

    public async Task<GetCohortsResponse> GetCohorts(GetCohortsRequest request)
    {
        var url = $"{BaseUrl()}api/cohorts/?accountId={request.AccountId}";

        logger.LogInformation("Getting GetCohorts {Url}", url);

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
        await AddAuthenticationHeader(requestMessage);

        using var response = await httpClient.SendAsync(requestMessage).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JsonConvert.DeserializeObject<GetCohortsResponse>(json);
    }

    public async Task<GetDraftApprenticeshipsResponse> GetDraftApprenticeships(long cohortId)
    {
        var url = $"{BaseUrl()}api/cohorts/{cohortId}/draft-apprenticeships";

        logger.LogInformation("Getting GetDraftApprenticeships {Url}", url);

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
        await AddAuthenticationHeader(requestMessage);

        using var response = await httpClient.SendAsync(requestMessage).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JsonConvert.DeserializeObject<GetDraftApprenticeshipsResponse>(json);
    }

    public async Task<GetApprenticeshipStatusSummaryResponse> GetEmployerAccountSummary(long accountId)
    {
        var url = $"{BaseUrl()}api/accounts/{accountId}/summary";

        logger.LogInformation("Getting GetEmployerAccountSummary {Url}", url);

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
        await AddAuthenticationHeader(requestMessage);

        using var response = await httpClient.SendAsync(requestMessage).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JsonConvert.DeserializeObject<GetApprenticeshipStatusSummaryResponse>(json);
    }

    private string BaseUrl()
    {
        if (config.ApiBaseUrl.EndsWith("/"))
        {
            return config.ApiBaseUrl;
        }

        return config.ApiBaseUrl + "/";
    }

    private async Task AddAuthenticationHeader(HttpRequestMessage httpRequestMessage)
    {
        if (!string.IsNullOrEmpty(config.IdentifierUri))
        {
            var accessToken = await azureCredentialHelper.GetAccessTokenAsync(config.IdentifierUri);
            
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
    }
}