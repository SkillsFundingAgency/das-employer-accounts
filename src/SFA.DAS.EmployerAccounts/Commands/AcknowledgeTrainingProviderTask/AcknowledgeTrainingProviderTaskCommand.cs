namespace SFA.DAS.EmployerAccounts.Commands.AcknowledgeTrainingProviderTask;

public class AcknowledgeTrainingProviderTaskCommand : IRequest
{
    public long AccountId { get; private set; }

    public AcknowledgeTrainingProviderTaskCommand(long accountId)
    {
        AccountId = accountId;
    }
}