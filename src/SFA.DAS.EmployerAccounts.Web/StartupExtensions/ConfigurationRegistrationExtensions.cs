﻿using Microsoft.Extensions.Options;
using NLog.Fluent;
using SFA.DAS.Activities.Client;
using SFA.DAS.Activities.IndexMappers;
using SFA.DAS.Audit.Client;
using SFA.DAS.Authorization.EmployerFeatures.Configuration;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.Elastic;
using SFA.DAS.EmployerAccounts.ReadStore.Configuration;
using SFA.DAS.Hmrc.Configuration;
using SFA.DAS.Notifications.Api.Client.Configuration;
using SFA.DAS.ReferenceData.Api.Client;
using SFA.DAS.TokenService.Api.Client;

namespace SFA.DAS.EmployerAccounts.Web.StartupExtensions;

public static class ConfigurationRegistrationExtensions
{
    public static IServiceCollection AddConfigurationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();

        services.Configure<EmployerAccountsConfiguration>(configuration.GetSection(nameof(EmployerAccountsConfiguration)));
        services.AddSingleton(cfg => cfg.GetService<IOptions<EmployerAccountsConfiguration>>().Value);

        services.Configure<EmployerAccountsOuterApiConfiguration>(configuration.GetSection(nameof(EmployerAccountsOuterApiConfiguration)));
        services.AddSingleton(cfg => cfg.GetService<IOptions<EmployerAccountsOuterApiConfiguration>>().Value);

        services.Configure<EmployerAccountsReadStoreConfiguration>(configuration.GetSection(nameof(EmployerAccountsReadStoreConfiguration)));
        services.AddSingleton(cfg => cfg.GetService<IOptions<EmployerAccountsReadStoreConfiguration>>().Value);

        services.Configure<ReferenceDataApiClientConfiguration>(configuration.GetSection(nameof(ReferenceDataApiClientConfiguration)));
        services.AddSingleton(cfg => cfg.GetService<IOptions<ReferenceDataApiClientConfiguration>>().Value);

        services.Configure<EmployerFeaturesConfiguration>(configuration.GetSection(nameof(EmployerFeaturesConfiguration)));
        services.AddSingleton(cfg => cfg.GetService<IOptions<EmployerFeaturesConfiguration>>().Value);

        services.Configure<AccountApiConfiguration>(configuration.GetSection(nameof(AccountApiConfiguration)));
        services.AddSingleton(cfg => cfg.GetService<IOptions<AccountApiConfiguration>>().Value);

        services.Configure<IdentityServerConfiguration>(configuration.GetSection(nameof(IdentityServerConfiguration)));
        services.AddSingleton(cfg => cfg.GetService<IOptions<IdentityServerConfiguration>>().Value);

        services.Configure<IAuditApiConfiguration>(configuration.GetSection(nameof(AuditApiClientConfiguration)));
        services.AddSingleton(cfg => cfg.GetService<IOptions<IAuditApiConfiguration>>().Value);

        services.Configure<ActivitiesClientConfiguration>(configuration.GetSection(nameof(ActivitiesClientConfiguration)));
        services.AddSingleton(cfg => cfg.GetService<IOptions<ActivitiesClientConfiguration>>().Value);

        services.AddSingleton<ElasticConfiguration>(cfg =>
        {
            var config = cfg.GetService<ActivitiesClientConfiguration>();
            return GetElasticConfiguration(config);
        });

        services.Configure<IAccountApiConfiguration>(configuration.GetSection(nameof(AccountApiConfiguration)));
        services.AddSingleton<IAccountApiConfiguration, AccountApiConfiguration>();

        services.Configure<INotificationsApiClientConfiguration>(configuration.GetSection(nameof(NotificationsApiClientConfiguration)));
        services.AddSingleton(cfg => cfg.GetService<IOptions<NotificationsApiClientConfiguration>>().Value);

        services.Configure<IReferenceDataApiConfiguration>(configuration.GetSection(nameof(ReferenceDataApiClientConfiguration)));
        services.AddSingleton(cfg => cfg.GetService<IOptions<ReferenceDataApiClientConfiguration>>().Value);

        services.Configure<ITokenServiceApiClientConfiguration>(configuration.GetSection(nameof(TokenServiceApiClientConfiguration)));

        var employerAccountsConfiguration = configuration.GetSection(nameof(EmployerAccountsConfiguration)) as EmployerAccountsConfiguration;

        services.AddSingleton<IHmrcConfiguration>(_ => employerAccountsConfiguration.Hmrc);
        services.AddSingleton(_ => employerAccountsConfiguration.TokenServiceApi);
        services.AddSingleton(_ => employerAccountsConfiguration.TasksApi);

        return services;
    }

    private static ElasticConfiguration GetElasticConfiguration(ActivitiesClientConfiguration activitiesdClientConfig)
    {
        var elasticConfig = new ElasticConfiguration()
            .UseSingleNodeConnectionPool(activitiesdClientConfig.ElasticUrl)
            .ScanForIndexMappers(typeof(ActivitiesIndexMapper).Assembly)
            .OnRequestCompleted(r => Log.Debug(r.DebugInformation));

        if (!string.IsNullOrWhiteSpace(activitiesdClientConfig.ElasticUsername) && !string.IsNullOrWhiteSpace(activitiesdClientConfig.ElasticPassword))
        {
            elasticConfig.UseBasicAuthentication(activitiesdClientConfig.ElasticUsername, activitiesdClientConfig.ElasticPassword);
        }

        return elasticConfig;
    }
}