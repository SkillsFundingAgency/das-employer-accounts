using NServiceBus;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Extensions;
using SFA.DAS.EmployerAccounts.Jobs.Services;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.Hosting;
using SFA.DAS.NServiceBus.Services;
using SFA.DAS.NServiceBus.SqlServer.Configuration;

namespace SFA.DAS.EmployerAccounts.Jobs.Extensions;

public static class ServiceCollectionExtensions
{
    private const string EndpointName = "SFA.DAS.EmployerAccounts.Jobs";

    public static IServiceCollection AddNServiceBus(
        this IServiceCollection services,
        EmployerAccountsConfiguration employerAccountsConfiguration,
        IConfiguration configuration)
    {
        var isLocal = configuration["EnvironmentName"]?.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase) ?? false;

        return services
            .AddSingleton(_ =>
            {
                var endpointConfiguration = new EndpointConfiguration(EndpointName)
                    .ConfigureServiceBusTransport(() => employerAccountsConfiguration.ServiceBusConnectionString, isLocal)
                    .UseErrorQueue($"{EndpointName}-errors")
                    .UseInstallers()
                    .UseSqlServerPersistence(() => DatabaseExtensions.GetSqlConnection(employerAccountsConfiguration.DatabaseConnectionString))
                    .UseNewtonsoftJsonSerializer()
                    .UseSendOnly();

                if (!string.IsNullOrEmpty(employerAccountsConfiguration.NServiceBusLicense))
                {
                    endpointConfiguration.UseLicense(employerAccountsConfiguration.NServiceBusLicense);
                }

                var endpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();

                return endpoint;
            })
            .AddSingleton<IMessageSession>(s => s.GetService<IEndpointInstance>())
            .AddSingleton<IEventPublisher, JobsEventPublisher>()
            .AddHostedService<NServiceBusHostedService>();
    }
}
