﻿using System.Configuration;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EmployerAccounts.Commands.AcceptInvitation;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.MessageHandlers.ServiceRegistrations;
using SFA.DAS.EmployerAccounts.MessageHandlers.Startup;
using SFA.DAS.EmployerAccounts.ReadStore.Application.Commands;
using SFA.DAS.EmployerAccounts.ServiceRegistration;
using SFA.DAS.UnitOfWork.DependencyResolution.Microsoft;

namespace SFA.DAS.EmployerAccounts.MessageHandlers.Extensions;

public static class HostBuilderExtensions
{
    public static IHostBuilder UseDasEnvironment(this IHostBuilder hostBuilder)
    {
        var environmentName = Environment.GetEnvironmentVariable(EnvironmentVariableNames.EnvironmentName);
        var mappedEnvironmentName = DasEnvironmentName.Map[environmentName];

        return hostBuilder.UseEnvironment(mappedEnvironmentName);
    }

    public static IHostBuilder ConfigureDasServices(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices((context,services) =>
        {
            services.Configure<EmployerAccountsConfiguration>(context.Configuration.GetSection(ConfigurationKeys.EmployerAccounts));
            services.AddSingleton(cfg => cfg.GetService<IOptions<EmployerAccountsConfiguration>>().Value);

            services.AddApplicationServices();
            services.AddUnitOfWork();
            services.AddNServiceBus();
            services.AddMemoryCache();
            services.AddCachesRegistrations();
            services.AddDatabaseRegistration();
            services.AddMediatR(
                typeof(UpdateAccountUserCommand),
                typeof(AcceptInvitationCommand)
            );
        });

        return hostBuilder;
    }
    public static IHostBuilder ConfigureDasLogging(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureLogging((context, loggingBuilder) =>
        {
            loggingBuilder.AddConsole(x => { });

            var appInsightsKey = context.Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
            if (!string.IsNullOrEmpty(appInsightsKey))
            {
                loggingBuilder.AddNLog(context.HostingEnvironment.IsDevelopment()
                    ? "nlog.development.config"
                    : "nlog.config");
                loggingBuilder.AddApplicationInsightsWebJobs(o => o.InstrumentationKey = appInsightsKey);
            }
        });

        return hostBuilder;
    }

    public static IHostBuilder ConfigureDasAppConfiguration(this IHostBuilder hostBuilder, string[] args)
    {
        return hostBuilder.ConfigureAppConfiguration((context, builder) =>
        {
            builder.AddAzureTableStorage(ConfigurationKeys.EmployerAccounts)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables()
                .AddCommandLine(args); ;
        });
    }
}