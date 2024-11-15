using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Commands.AccountLevyStatus;
using SFA.DAS.EmployerFinance.Messages.Events;

namespace SFA.DAS.EmployerAccounts.MessageHandlers.EventHandlers.EmployerFinance;

public class RefreshEmployerLevyDataCompletedEventHandler(IMediator mediator) : IHandleMessages<RefreshEmployerLevyDataCompletedEvent>
{  
    public async Task Handle(RefreshEmployerLevyDataCompletedEvent message, IMessageHandlerContext context)
    {

        await mediator.Send(new AccountLevyStatusCommand
        {
            AccountId = message.AccountId,
            ApprenticeshipEmployerType = message.LevyTransactionValue == decimal.Zero ? ApprenticeshipEmployerType.NonLevy : ApprenticeshipEmployerType.Levy
        });
    }
}