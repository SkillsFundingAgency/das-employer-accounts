using Microsoft.Extensions.Logging.ApplicationInsights;
using NLog.Extensions.Logging;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EmployerAccounts.Commands.AccountLevyStatus;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.MessageHandlers.ServiceRegistrations;
using SFA.DAS.EmployerAccounts.MessageHandlers.Startup;
using SFA.DAS.EmployerAccounts.ReadStore.Application.Commands;
using SFA.DAS.EmployerAccounts.ReadStore.ServiceRegistrations;
using SFA.DAS.EmployerAccounts.ServiceRegistration;
using SFA.DAS.EmployerAccounts.Startup;
using SFA.DAS.UnitOfWork.DependencyResolution.Microsoft;
using SFA.DAS.UnitOfWork.NServiceBus.DependencyResolution.Microsoft;

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
        hostBuilder.ConfigureServices((context, services) =>
        {
            var employerAccountsConfiguration = context.Configuration.GetSection(ConfigurationKeys.EmployerAccounts).Get<EmployerAccountsConfiguration>();

            services.AddConfigurationSections(context.Configuration);

            services
            .AddUnitOfWork()
            .AddEntityFramework(employerAccountsConfiguration);

            services.AddNotifications(context.Configuration);

            services.AddApplicationServices();
            services.AddReadStoreServices();
            services.AddMessageHandlerDataRepositories();
            services.AddMediatorValidators();
            services.AddNServiceBusUnitOfWork();
            services.AddMemoryCache();
            services.AddCachesRegistrations();
            services.AddDatabaseRegistration();
            services.AddEventsApi();
            services.AddAuditServices();
            services.AddHttpContextAccessor();
            services.AddMediatR(serviceConfiguration=> serviceConfiguration.RegisterServicesFromAssemblies(
                typeof(CreateAccountUserCommandHandler).Assembly,
                typeof(AccountLevyStatusCommandHandler).Assembly)
            );
            services.AddNServiceBus();
        });

        return hostBuilder;
    }

    public static IHostBuilder ConfigureDasLogging(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureLogging((context, loggingBuilder) =>
        {
            var connectionString = context.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
            if (!string.IsNullOrEmpty(connectionString))
            {
                loggingBuilder.AddNLog(context.HostingEnvironment.IsDevelopment()
                    ? "nlog.development.config"
                    : "nlog.config");
                loggingBuilder.AddApplicationInsightsWebJobs(o => o.ConnectionString = connectionString);
                loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Information);
                loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Information);
            }

            loggingBuilder.AddConsole();
        });

        return hostBuilder;
    }

    public static IHostBuilder ConfigureDasAppConfiguration(this IHostBuilder hostBuilder, string[] args)
    {
        return hostBuilder.ConfigureAppConfiguration((context, builder) =>
        {
            builder.AddAzureTableStorage(options =>
                    {
                        options.ConfigurationKeys = new[]
                            { ConfigurationKeys.EmployerAccounts, ConfigurationKeys.EmployerAccountsReadStore, ConfigurationKeys.EncodingConfig, ConfigurationKeys.NotificationsApiClient };
                        options.PreFixConfigurationKeys = true;
                        options.ConfigurationKeysRawJsonResult = new[] { ConfigurationKeys.EncodingConfig };
                    }
                )
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables()
                .AddCommandLine(args);
        });
    }
}