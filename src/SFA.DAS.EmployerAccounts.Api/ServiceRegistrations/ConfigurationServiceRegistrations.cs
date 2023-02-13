﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SFA.DAS.Authorization.EmployerFeatures.Configuration;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Api.ServiceRegistrations;

public static  class ConfigurationServiceRegistrations
{
    public static IServiceCollection AddApiConfigurationSections(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();
        services.Configure<EmployerAccountsConfiguration>(configuration.GetSection(ConfigurationKeys.EmployerAccounts));

        var employerAccountsConfiguration = configuration.Get<EmployerAccountsConfiguration>();
        services.AddSingleton(employerAccountsConfiguration);

        var encodingConfigJson = configuration.GetSection("SFA.DAS.Encoding").Value;
        var encodingConfig = JsonConvert.DeserializeObject<EncodingConfig>(encodingConfigJson);
        services.AddSingleton(encodingConfig);

        services.Configure<EmployerFeaturesConfiguration>(configuration.GetSection(ConfigurationKeys.Features));
        services.AddSingleton(cfg => cfg.GetService<IOptions<EmployerFeaturesConfiguration>>().Value);

        return services;
    }
}