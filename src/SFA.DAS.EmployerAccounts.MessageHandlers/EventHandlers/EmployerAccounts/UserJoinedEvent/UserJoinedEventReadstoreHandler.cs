using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.EmployerAccounts.ReadStore.Application.Commands;

namespace SFA.DAS.EmployerAccounts.MessageHandlers.EventHandlers.EmployerAccounts;

public class UserJoinedEventReadstoreHandler : IHandleMessages<UserJoinedEvent>
{
    private readonly IMediator _mediator;

    public UserJoinedEventReadstoreHandler(IMediator mediator)
    {
        _mediator = mediator;
    }
    public async Task Handle(UserJoinedEvent message, IMessageHandlerContext context)
    {
        await _mediator.Send(new CreateAccountUserCommand(message.AccountId, message.UserRef, message.Role, context.MessageId, message.Created));
    }
}