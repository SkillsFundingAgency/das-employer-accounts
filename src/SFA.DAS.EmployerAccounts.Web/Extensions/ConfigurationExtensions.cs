using System.IO;
using System.Text.Json;
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

        // LOCAL table-storage rows often point DatabaseConnectionString at cloud TEST SQL
        // and Encoding at TEST salts. Re-assert Isolation infrastructure (SQL / WireMock /
        // Encoding salts for seeded GP67XW) while HMRC ClientId/Secret/BaseUrl/Scope remain
        // from table storage.
        if (isolationMode)
        {
            configurationBuilder.AddInMemoryCollection(LoadIsolationInfrastructureOverrides());
        }

        return configurationBuilder.Build();
    }

    private static IEnumerable<KeyValuePair<string, string>> LoadIsolationInfrastructureOverrides()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Isolation.json");
        if (!File.Exists(path))
        {
            yield break;
        }

        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        if (!doc.RootElement.TryGetProperty("SFA.DAS.EmployerAccounts", out var eas))
        {
            yield break;
        }

        foreach (var name in new[]
                 {
                     "DatabaseConnectionString",
                     "DataProtectionKeysDatabase",
                     "RedisConnectionString",
                     "ServiceBusConnectionString",
                     "NServiceBusLicense"
                 })
        {
            if (eas.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
            {
                yield return new KeyValuePair<string, string>($"SFA.DAS.EmployerAccounts:{name}", value.GetString());
            }
        }

        foreach (var section in new[]
                 {
                     "EmployerAccountsOuterApiConfiguration",
                     "AccountApi",
                     "ContentApi",
                     "CommitmentsApi",
                     "PensionRegulatorApi",
                     "ProviderRegistrationsApi",
                     "RecruitApi"
                 })
        {
            if (!eas.TryGetProperty(section, out var obj) || obj.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            foreach (var prop in obj.EnumerateObject())
            {
                if (prop.Value.ValueKind == JsonValueKind.String)
                {
                    yield return new KeyValuePair<string, string>(
                        $"SFA.DAS.EmployerAccounts:{section}:{prop.Name}",
                        prop.Value.GetString());
                }
            }
        }

        // Prefer empty Token Service client secrets so Azure CLI / MI-style auth is used locally.
        if (eas.TryGetProperty("TokenServiceApi", out var token) && token.ValueKind == JsonValueKind.Object)
        {
            foreach (var name in new[] { "ClientId", "ClientSecret" })
            {
                if (token.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
                {
                    yield return new KeyValuePair<string, string>(
                        $"SFA.DAS.EmployerAccounts:TokenServiceApi:{name}",
                        value.GetString());
                }
            }
        }

        // LOCAL Encoding from Azurite uses TEST salts; Isolation seed hashes (GP67XW) use
        // Isolation salts. Re-assert Isolation Encoding so Decode(hashedAccountId) works.
        if (doc.RootElement.TryGetProperty("SFA.DAS.Encoding", out var encoding))
        {
            var encodingJson = encoding.ValueKind == JsonValueKind.String
                ? encoding.GetString()
                : encoding.GetRawText();
            if (!string.IsNullOrWhiteSpace(encodingJson))
            {
                yield return new KeyValuePair<string, string>("SFA.DAS.Encoding", encodingJson);
            }
        }

        // Older Hashstring settings on EmployerAccounts also affect some paths — keep Isolation.
        foreach (var name in new[] { "Hashstring", "AllowedHashstringCharacters" })
        {
            if (eas.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
            {
                yield return new KeyValuePair<string, string>($"SFA.DAS.EmployerAccounts:{name}", value.GetString());
            }
        }
    }
}
