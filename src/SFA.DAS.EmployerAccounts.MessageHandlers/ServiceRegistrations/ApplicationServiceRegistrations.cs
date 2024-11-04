using SFA.DAS.Api.Common.Infrastructure;
using SFA.DAS.Api.Common.Interfaces;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Services;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.MessageHandlers.ServiceRegistrations;

public static class ApplicationServiceRegistrations
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IEncodingService, EncodingService>();
        services.AddTransient<IAzureClientCredentialHelper, AzureClientCredentialHelper>();
        services.AddScoped<ITopicClientFactory, TopicClientFactory>();
        services.AddScoped<ILegacyTopicMessagePublisher>(sp =>
        {
            var clientFactory = sp.GetService<ITopicClientFactory>();
            var logger = sp.GetService<ILogger<LegacyTopicMessagePublisher>>();
            var config = sp.GetService<EmployerAccountsConfiguration>();
            
            return new LegacyTopicMessagePublisher(clientFactory, logger, config.MessageServiceBusConnectionString);
        });

        return services;
    }
}