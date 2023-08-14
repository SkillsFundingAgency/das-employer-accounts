using SFA.DAS.EmployerAccounts.Events.Messages;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Messages.Events;

namespace SFA.DAS.EmployerAccounts.MessageHandlers.EventHandlers.EmployerAccounts;

public class UserJoinedEventLegacyPublishHandler : IHandleMessages<UserJoinedEvent>
{
    private readonly ILegacyTopicMessagePublisher _messagePublisher;

    public UserJoinedEventLegacyPublishHandler(ILegacyTopicMessagePublisher messagePublisher)
    {
        _messagePublisher = messagePublisher;
    }
    
    public async Task Handle(UserJoinedEvent message, IMessageHandlerContext context)
    {
        await _messagePublisher.PublishAsync(
            new UserJoinedMessage(
                message.AccountId,
                message.UserName,
                message.UserRef.ToString()));
    }
}