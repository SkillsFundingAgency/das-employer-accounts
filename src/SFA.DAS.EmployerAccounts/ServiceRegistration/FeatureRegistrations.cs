using Microsoft.Extensions.DependencyInjection;

namespace SFA.DAS.EmployerAccounts.ServiceRegistration;

public static class FeatureRegistrations
{
    public static void AddFeatureToggle(this IServiceCollection services)
    {
        services.AddSingleton<IFeature, Feature>();
    }
}