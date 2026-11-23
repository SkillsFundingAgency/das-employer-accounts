using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Services;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Api.ServiceRegistrations;

public static class ApplicationServiceRegistrations
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IAccountApiConfiguration>(serviceProvider =>
            serviceProvider.GetRequiredService<EmployerAccountsConfiguration>().AccountApi);
        services.AddScoped<IAccountApiClient, AccountApiClient>();
        services.AddTransient<IPayeSchemesService, PayeSchemesService>();
        services.AddTransient<IUserAornPayeLockService, UserAornPayeLockService>();
        services.AddTransient<IEncodingService, EncodingService>();
        return services;
    }
}
