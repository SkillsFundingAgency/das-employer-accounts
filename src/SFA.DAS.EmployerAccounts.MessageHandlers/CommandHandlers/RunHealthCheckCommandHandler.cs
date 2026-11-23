using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.EmployerAccounts.Messages.Commands;

namespace SFA.DAS.EmployerAccounts.MessageHandlers.CommandHandlers;

public class RunHealthCheckCommandHandler(
    IDistributedCache distributedCache,
    ILogger<RunHealthCheckCommandHandler> logger)
    : IHandleMessages<RunHealthCheckCommand>
{
    public Task Handle(RunHealthCheckCommand message, IMessageHandlerContext context)
    {
        logger.LogInformation("Handled {TypeName} with MessageId '{MessageId}'", nameof(RunHealthCheckCommand), context.MessageId);

        return distributedCache.SetStringAsync(context.MessageId, "OK");
    }
}
