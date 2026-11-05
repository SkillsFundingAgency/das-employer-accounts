using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmployerAccounts.Configuration;

namespace SFA.DAS.EmployerAccounts.ServiceRegistration;

public static class FeatureRegistrations
{
    public static void AddFeatureToggle(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IFeature>(new Feature(
            configuration
                .GetSection(ConfigurationKeys.EmployerAccounts)
                .GetSection(nameof(EmployerAccountsConfiguration.Features))
        ));
    }
}