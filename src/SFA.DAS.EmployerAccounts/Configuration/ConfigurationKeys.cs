﻿namespace SFA.DAS.EmployerAccounts.Configuration;

public static class ConfigurationKeys
{
    public const string ServiceName = "SFA.DAS.EmployerAccounts";

    public static string EmployerAccounts => ServiceName;
    public const string EmployerAccountsApiClient = $"{ServiceName}.Api.Client";
    public const string EmployerAccountsReadStore = $"{ServiceName}.ReadStore";
    public const string Features = $"{ServiceName}.Features";
    public const string AzureActiveDirectoryApiConfiguration = "AzureADApiAuthentication";
    public const string AuditApi = "SFA.DAS.AuditApiClient";
    public const string EncodingConfig = "SFA.DAS.Encoding";
}