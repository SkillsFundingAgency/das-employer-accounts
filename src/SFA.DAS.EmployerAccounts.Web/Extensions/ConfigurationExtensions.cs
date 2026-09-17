using System.IO;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EmployerAccounts.Configuration;

namespace SFA.DAS.EmployerAccounts.Web.Extensions;

public static class ConfigurationExtensions
{
    public static IConfiguration BuildDasConfiguration(this IConfiguration configuration)
    {
        var configurationBuilder = new ConfigurationBuilder()
            .AddConfiguration(configuration)
            .SetBasePath(Directory.GetCurrentDirectory());

#if DEBUG
        if (!configuration.IsDev())
        {
            configurationBuilder
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile("appsettings.Development.json", true);
        }
#endif

        // Isolation: load local FAKE config and skip Azure Table Storage / das-employer-config.
        var isolationMode = string.Equals(configuration["IsolationMode"], "true", StringComparison.OrdinalIgnoreCase);
        if (isolationMode)
        {
            configurationBuilder.AddJsonFile("appsettings.Isolation.json", optional: false, reloadOnChange: true);
        }

        configurationBuilder.AddEnvironmentVariables();

        if (!isolationMode)
        {
            configurationBuilder.AddAzureTableStorage(options =>
                {
                    options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
                    options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                    options.EnvironmentName = configuration["EnvironmentName"];
                    options.PreFixConfigurationKeys = true;
                    options.ConfigurationKeysRawJsonResult = [ConfigurationKeys.EncodingConfig];
                }
            );
        }

        return configurationBuilder.Build();
    }
}
