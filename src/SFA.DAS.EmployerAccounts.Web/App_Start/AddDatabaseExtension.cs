﻿using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerAccounts.Data;

namespace SFA.DAS.EmployerAccounts.Web;

public static class AddDatabaseExtension
{
    public static void AddDatabaseRegistration(this IServiceCollection services, EmployerAccountsConfiguration config, string environmentName)
    {
        if (environmentName.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
        {
            services.AddDbContext<EmployerAccountsDbContext>(options => options.UseSqlServer(config.DatabaseConnectionString), ServiceLifetime.Transient);
        }
        else
        {
            services.AddDbContext<EmployerAccountsDbContext>(ServiceLifetime.Transient);
        }

        services.AddTransient<EmployerAccountsDbContext, EmployerAccountsDbContext>(provider => provider.GetService<EmployerAccountsDbContext>());
        services.AddTransient(provider => new Lazy<EmployerAccountsDbContext>(provider.GetService<EmployerAccountsDbContext>()));
    }
}