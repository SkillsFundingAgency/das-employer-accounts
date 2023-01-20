﻿using Microsoft.Extensions.Options;
using SFA.DAS.Audit.Client;
using SFA.DAS.Authorization.EmployerFeatures.Configuration;
using SFA.DAS.EmployerAccounts.ReadStore.Configuration;
using SFA.DAS.Encoding;
using SFA.DAS.Hmrc.Configuration;
using SFA.DAS.ReferenceData.Api.Client;
using SFA.DAS.TokenService.Api.Client;

namespace SFA.DAS.EmployerAccounts.Web.StartupExtensions;

public static class ConfigurationServiceRegistrations
{
    public static IServiceCollection AddConfigurationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmployerAccountsConfiguration>(configuration.GetSection(ConfigurationKeys.EmployerAccounts));
        services.AddSingleton(cfg => cfg.GetService<IOptions<EmployerAccountsConfiguration>>().Value);

        services.Configure<EmployerAccountsReadStoreConfiguration>(configuration.GetSection(nameof(EmployerAccountsReadStoreConfiguration)));
        services.AddSingleton(cfg => cfg.GetService<IOptions<EmployerAccountsReadStoreConfiguration>>().Value);

        services.Configure<ReferenceDataApiClientConfiguration>(configuration.GetSection(ConfigurationKeys.ReferenceDataApiClient));
        services.AddSingleton(cfg => cfg.GetService<IOptions<ReferenceDataApiClientConfiguration>>().Value);

        services.Configure<EmployerFeaturesConfiguration>(configuration.GetSection(ConfigurationKeys.Features));
        services.AddSingleton(cfg => cfg.GetService<IOptions<EmployerFeaturesConfiguration>>().Value);

        //services.Configure<AccountApiConfiguration>(configuration.GetSection(ConfigurationKeys.a));
        //services.AddSingleton(cfg => cfg.GetService<IOptions<AccountApiConfiguration>>().Value);

        //services.Configure<IAccountApiConfiguration>(configuration.GetSection(nameof(AccountApiConfiguration)));
        //services.AddSingleton<IAccountApiConfiguration, AccountApiConfiguration>();

        services.Configure<IdentityServerConfiguration>(configuration.GetSection("Identity"));
        services.AddSingleton(cfg => cfg.GetService<IOptions<IdentityServerConfiguration>>().Value);

        services.Configure<IAuditApiConfiguration>(configuration.GetSection(ConfigurationKeys.AuditApi));
        services.AddSingleton(cfg => cfg.GetService<IOptions<IAuditApiConfiguration>>().Value);

        services.Configure<EncodingConfig>(configuration.GetSection(ConfigurationKeys.EncodingConfig));
        services.AddSingleton(cfg => cfg.GetService<IOptions<EncodingConfig>>().Value);

        services.Configure<IReferenceDataApiConfiguration>(configuration.GetSection(nameof(ReferenceDataApiClientConfiguration)));
        services.AddSingleton(cfg => cfg.GetService<IOptions<ReferenceDataApiClientConfiguration>>().Value);

        services.Configure<ITokenServiceApiClientConfiguration>(configuration.GetSection(nameof(TokenServiceApiClientConfiguration)));

        var employerAccountsConfiguration = configuration.GetSection(nameof(EmployerAccountsConfiguration)) as EmployerAccountsConfiguration;

        services.AddSingleton<IHmrcConfiguration>(_ => employerAccountsConfiguration.Hmrc);
        services.AddSingleton(_ => employerAccountsConfiguration.TokenServiceApi);
        services.AddSingleton(_ => employerAccountsConfiguration.TasksApi);
        services.AddSingleton(_ => employerAccountsConfiguration.CommitmentsApi);

        return services;
    }
}