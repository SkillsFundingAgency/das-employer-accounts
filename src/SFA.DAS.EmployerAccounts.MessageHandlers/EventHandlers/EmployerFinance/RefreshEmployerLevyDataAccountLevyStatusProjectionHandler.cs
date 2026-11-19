using SFA.DAS.EmployerAccounts.Commands.LevyDormancy;
using SFA.DAS.EmployerFinance.Messages.Events;

namespace SFA.DAS.EmployerAccounts.MessageHandlers.EventHandlers.EmployerFinance;

public class RefreshEmployerLevyDataAccountLevyStatusProjectionHandler(IMediator mediator)
    : IHandleMessages<RefreshEmployerLevyDataCompletedEvent>
{
    public Task Handle(RefreshEmployerLevyDataCompletedEvent message, IMessageHandlerContext context)
    {
        return mediator.Send(new UpsertEmployerAccountLevyStatusCommand
        {
            AccountId = message.AccountId,
            LastLevyDeclarationDate = message.LastLevyDeclarationDate,
            RefreshedAt = message.Created
        });
    }
}
