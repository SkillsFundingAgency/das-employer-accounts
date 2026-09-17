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

        // Isolation: load local FAKE stub URLs. Still load Azure Table Storage afterwards
        // (when a connection string is set) so HMRC ClientId/Secret/BaseUrl/Scope and related
        // values from das-employer-config (local Azurite / UseDevelopmentStorage) override
        // Isolation defaults — do not paste secrets into appsettings.Isolation.json.
        var isolationMode = string.Equals(configuration["IsolationMode"], "true", StringComparison.OrdinalIgnoreCase);
        if (isolationMode)
        {
            configurationBuilder.AddJsonFile("appsettings.Isolation.json", optional: false, reloadOnChange: true);
        }

        configurationBuilder.AddEnvironmentVariables();

        var storageConnectionString = configuration["ConfigurationStorageConnectionString"];
        var configNames = configuration["ConfigNames"];
        if (!string.IsNullOrWhiteSpace(storageConnectionString) && !string.IsNullOrWhiteSpace(configNames))
        {
            configurationBuilder.AddAzureTableStorage(options =>
                {
                    options.ConfigurationKeys = configNames.Split(",");
                    options.StorageConnectionString = storageConnectionString;
                    options.EnvironmentName = configuration["EnvironmentName"];
                    options.PreFixConfigurationKeys = true;
                    options.ConfigurationKeysRawJsonResult = [ConfigurationKeys.EncodingConfig];
                }
            );
        }

        return configurationBuilder.Build();
    }
}
