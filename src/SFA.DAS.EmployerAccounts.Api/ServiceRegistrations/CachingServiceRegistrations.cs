using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmployerAccounts.Configuration;
using StackExchange.Redis;

namespace SFA.DAS.EmployerAccounts.Api.ServiceRegistrations;

public static class CachingServiceRegistrations
{
    private const int RedisConnectionTimeoutMilliseconds = 15000;
    
    public static IServiceCollection AddDasDistributedMemoryCache(this IServiceCollection services, EmployerAccountsConfiguration configuration, bool isDevelopment)
    {
        if (isDevelopment)
        {
            services.AddDistributedMemoryCache();
        }
        else
        {
            services.AddStackExchangeRedisCache(redisCacheOptions =>
            {
                var config = ConfigurationOptions.Parse(configuration.RedisConnectionString);
                config.ConnectTimeout = RedisConnectionTimeoutMilliseconds;
                
                redisCacheOptions.ConfigurationOptions = config;
            });
        }

        return services;
    }
}