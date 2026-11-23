using NServiceBus;
using SFA.DAS.EmployerAccounts.Commands.RunHealthCheckCommand;
using SFA.DAS.EmployerFinance.Messages.Commands;
using SFA.DAS.Notifications.Messages.Commands;

namespace SFA.DAS.EmployerAccounts.Extensions;

public static class RoutingExtensions
{
    public static void AddRouting(this RoutingSettings routing)
    {
        routing.RouteToEndpoint(
            typeof(SendEmailCommand).Assembly,
            typeof(SendEmailCommand).Namespace,
            "SFA.DAS.Notifications.MessageHandlers"
        );

        routing.RouteToEndpoint(
            typeof(ImportLevyDeclarationsCommand).Assembly,
            typeof(ImportLevyDeclarationsCommand).Namespace,
            "SFA.DAS.EmployerFinance.MessageHandlers"
        );

        routing.RouteToEndpoint(
            typeof(RunHealthCheckCommand).Assembly,
            typeof(RunHealthCheckCommand).Namespace,
            "SFA.DAS.EmployerAccounts.MessageHandlers"
        );
    }
}