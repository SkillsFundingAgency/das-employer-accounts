using NServiceBus;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerAccounts.Jobs.Services;

public class JobsEventPublisher(IMessageSession messageSession) : IEventPublisher
{
    public Task Publish<T>(T message) where T : class => messageSession.Publish(message);

    public Task Publish<T>(Func<T> messageFactory) where T : class => messageSession.Publish(messageFactory());
}
