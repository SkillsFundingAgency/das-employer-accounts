﻿using SFA.DAS.AutoConfiguration;
using SFA.DAS.Caches;

namespace SFA.DAS.EmployerAccounts.Web.StartupExtensions;

public static class CachesServiceRegistrations
{
    public static IServiceCollection AddCachesRegistrations(this IServiceCollection services)
    {
        services.AddSingleton<IInProcessCache, InProcessCache>();

        services.AddSingleton(s =>
        {
            var environment = s.GetService<IEnvironmentService>();
            var config = s.GetService<EmployerAccountsConfiguration>();

            return environment.IsCurrent(DasEnv.LOCAL)
                ? new LocalDevCache() as IDistributedCache
                : new RedisCache(config.RedisConnectionString);
        });

        return services;
    }
}