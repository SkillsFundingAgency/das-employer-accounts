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

        // Isolation: load FAKE local URLs / Encoding / Docker SQL first.
        // Azure Table Storage still loads afterwards (when configured) so HMRC
        // ClientId/Secret from Azurite overlay Isolation defaults — do not put
        // those secrets in appsettings.Isolation.json.
        // Re-load Isolation JSON last so Docker SQL / Redis / WireMock / Encoding
        // win over LOCAL table-storage rows that point at cloud TEST.
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

        if (isolationMode)
        {
            configurationBuilder.AddJsonFile("appsettings.Isolation.json", optional: false, reloadOnChange: true);
        }

        return configurationBuilder.Build();
    }
}
