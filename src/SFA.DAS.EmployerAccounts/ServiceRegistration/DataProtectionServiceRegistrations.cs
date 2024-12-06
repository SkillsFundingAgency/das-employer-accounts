using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Infrastructure.DataProtection;
using StackExchange.Redis;

namespace SFA.DAS.EmployerAccounts.ServiceRegistration;

[ExcludeFromCodeCoverage]
public static class DataProtectionServiceRegistrations
{
    private const string ApplicationName = "das-employer";
    public static IServiceCollection AddDataProtection(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        var config = configuration.GetSection(ConfigurationKeys.ServiceName).Get<EmployerAccountsConfiguration>();

        if (isDevelopment)
        {
            services
            .AddDataProtection()
                .SetApplicationName(ApplicationName)
                .PersistKeysToFileSystem(new DirectoryInfo(@"C:\Esfa\SharedKeys"));
        }
        else
        {
            if (config != null && !string.IsNullOrEmpty(config.DataProtectionKeysDatabase) && !string.IsNullOrEmpty(config.RedisConnectionString))
            {
                var redisConnectionString = config.RedisConnectionString;
                var dataProtectionKeysDatabase = config.DataProtectionKeysDatabase;

                var redis = ConnectionMultiplexer.Connect($"{redisConnectionString},{dataProtectionKeysDatabase}");
                services.AddDataProtection()
                    .SetApplicationName(ApplicationName)
                    .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");
            }
        }

        services.AddTransient<IDataProtectorServiceFactory, DataProtectorServiceFactory>();

        return services;
    }
}
