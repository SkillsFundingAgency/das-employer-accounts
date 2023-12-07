using System.Threading;
using SFA.DAS.EmployerAccounts.Data.Contracts;

namespace SFA.DAS.EmployerAccounts.Commands.AcknowledgeTrainingProviderTask;

public class AcknowledgeTrainingProviderTaskCommandHandler : IRequestHandler<AcknowledgeTrainingProviderTaskCommand>
{
    private readonly IEmployerAccountRepository _employerAccountRepository;

    public AcknowledgeTrainingProviderTaskCommandHandler(IEmployerAccountRepository employerAccountRepository)
    {
        _employerAccountRepository = employerAccountRepository;
    }

    public async Task Handle(AcknowledgeTrainingProviderTaskCommand message, CancellationToken cancellationToken)
    {
        await _employerAccountRepository.AcknowledgeTrainingProviderTask(message.AccountId);
    }
}