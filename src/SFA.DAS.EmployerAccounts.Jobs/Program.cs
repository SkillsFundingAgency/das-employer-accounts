using SFA.DAS.EmployerAccounts.Jobs.Extensions;

namespace SFA.DAS.EmployerAccounts.Jobs;

public class Program
{
    public static async Task Main()
    {
        using (var host = CreateHost())
        {
            await host.RunAsync();
        }
    }

    private static IHost CreateHost()
    {
        return new HostBuilder()
            .ConfigureDasAppConfiguration()
            .ConfigureDasWebJobs()
            .ConfigureDasLogging()
            .ConfigureDasServices()
            .Build();
    }
}