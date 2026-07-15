using System.IO;
using Microsoft.Azure.WebJobs.Logging.ApplicationInsights;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EmployerAccounts.Commands.AccountLevyStatus;
using SFA.DAS.EmployerAccounts.Commands.SendNotification;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Data;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Jobs.ServiceRegistrations;
using SFA.DAS.EmployerAccounts.ServiceRegistration;
using SFA.DAS.EmployerAccounts.Validation;

namespace SFA.DAS.EmployerAccounts.Jobs.Extensions;

public static class HostExtensions
{
    public static IHostBuilder ConfigureDasWebJobs(this IHostBuilder builder)
    {
        builder.ConfigureWebJobs(config =>
        {
            config.AddTimers();
            config.AddAzureStorageCoreServices();
        });

        return builder;
    }

    public static IHostBuilder ConfigureDasLogging(this IHostBuilder builder)
    {
        builder.ConfigureLogging((context, loggingBuilder) =>
        {
            var connectionString = context.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
            if (!string.IsNullOrEmpty(connectionString))
            {
                loggingBuilder.AddApplicationInsightsWebJobs(o => o.ConnectionString = connectionString);
                loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Information);
                loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Information);
            }

            loggingBuilder.AddConsole();
        });

        return builder;
    }

    public static IHostBuilder ConfigureDasAppConfiguration(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureAppConfiguration((context, builder) =>
        {
            builder.AddConfiguration(context.Configuration).SetBasePath(Directory.GetCurrentDirectory());
            builder.AddAzureTableStorage(options =>
                {
                    options.ConfigurationKeys =
                    [
                        ConfigurationKeys.EmployerAccounts,
                        ConfigurationKeys.EncodingConfig
                    ];
                    options.PreFixConfigurationKeys = true;
                    options.ConfigurationKeysRawJsonResult = [ConfigurationKeys.EncodingConfig];
                })
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables();
        });
    }

    public static IHostBuilder ConfigureDasServices(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices((context, services) =>
        {
            services.AddConfigurationSections(context.Configuration);
            services.AddDateTimeServices(context.Configuration);

            var employerAccountsConfiguration = context.Configuration
                .GetSection(ConfigurationKeys.EmployerAccounts)
                .Get<EmployerAccountsConfiguration>();

            services.AddDatabaseRegistration();
            services.AddScoped<IEmployerAccountTeamRepository, EmployerAccountTeamRepository>();
            services.AddTransient<IValidator<SendNotificationCommand>, SendNotificationCommandValidator>();
            services.AddNServiceBus(employerAccountsConfiguration, context.Configuration);
            services.AddMediatR(serviceConfiguration => serviceConfiguration.RegisterServicesFromAssemblies(
                typeof(AccountLevyStatusCommandHandler).Assembly));
        });

        return hostBuilder;
    }
}
