﻿using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace SFA.DAS.EmployerAccounts.Web.Helpers;

public class UrlActionHelper : IUrlActionHelper
{
    private readonly EmployerAccountsConfiguration _configuration;
    private readonly IActionContextAccessor _actionContextAccessor;

    public UrlActionHelper(EmployerAccountsConfiguration configuration, IActionContextAccessor actionContextAccessor)
    {
        _configuration = configuration;
        _actionContextAccessor = actionContextAccessor;
    }

    public string EmployerAccountsAction(string path)
    {
        var baseUrl = _configuration.EmployerAccountsBaseUrl;

        return AccountAction(baseUrl, path);
    }

    public string EmployerCommitmentsV2Action(string path)
    {
        var baseUrl = _configuration.EmployerCommitmentsV2BaseUrl;

        return NonAccountsAction(baseUrl, path);
    }

    public string LevyTransfersMatchingAction(string path)
    {
        var baseUrl = _configuration.LevyTransferMatchingBaseUrl;

        return AccountAction(baseUrl, path);
    }

    public string ReservationsAction(string path)
    {
        var baseUrl = _configuration.ReservationsBaseUrl;

        return AccountAction(baseUrl, path);
    }

    public string EmployerFinanceAction(string path)
    {
        var baseUrl = _configuration.EmployerFinanceBaseUrl;

        return AccountAction(baseUrl, path);
    }

    public string EmployerIncentivesAction(string path = "")
    {
        var baseUrl = _configuration.EmployerIncentivesBaseUrl;
        var hashedAccountId = _actionContextAccessor.ActionContext.RouteData.Values[ControllerConstants.AccountHashedIdRouteKeyName];
        return Action(baseUrl, $"{hashedAccountId}/{path}");
    }

    public string EmployerProjectionsAction(string path)
    {
        var baseUrl = _configuration.EmployerProjectionsBaseUrl;

        return AccountAction(baseUrl, path);
    }

    public string EmployerRecruitAction(string path = "")
    {
        var baseUrl = _configuration.EmployerRecruitBaseUrl;

        return AccountAction(baseUrl, path);
    }

    public string ProviderRelationshipsAction(string path)
    {
        var baseUrl = _configuration.ProviderRelationshipsBaseUrl;

        return AccountAction(baseUrl, path);
    }

    public string LegacyEasAccountAction(string path)
    {
        var baseUrl = _configuration.EmployerPortalBaseUrl;

        return AccountAction(baseUrl, path);
    }

    public string LegacyEasAction(string path)
    {
        var baseUrl = _configuration.EmployerPortalBaseUrl;

        return Action(baseUrl, path);
    }

    private string AccountAction(string baseUrl, string path)
    {
        var hashedAccountId = _actionContextAccessor.ActionContext.RouteData.Values[ControllerConstants.AccountHashedIdRouteKeyName];
        var accountPath = hashedAccountId == null ? $"accounts/{path}" : $"accounts/{hashedAccountId}/{path}";

        return Action(baseUrl, accountPath);
    }

    // unlike the rest of the services within MA - commitments v2 does not have 'accounts/' in its urls
    // Nor does Employer Feedback
    private string NonAccountsAction(string baseUrl, string path)
    {

        var hashedAccountId = _actionContextAccessor.ActionContext.RouteData.Values[ControllerConstants.AccountHashedIdRouteKeyName];
        var commitmentPath = hashedAccountId == null ? $"{path}" : $"{hashedAccountId}/{path}";

        return Action(baseUrl, commitmentPath);
    }

    public string EmployerFeedbackAction(string path)
    {
        var baseUrl = _configuration.EmployerFeedbackBaseUrl;

        return NonAccountsAction(baseUrl, path);
    }

    private static string Action(string baseUrl, string path)
    {
        var trimmedBaseUrl = baseUrl?.TrimEnd('/') ?? string.Empty;

        return $"{trimmedBaseUrl}/{path}".TrimEnd('/');
    }
}