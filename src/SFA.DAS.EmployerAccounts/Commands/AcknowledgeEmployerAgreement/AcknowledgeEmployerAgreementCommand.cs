namespace SFA.DAS.EmployerAccounts.Commands.AcknowledgeEmployerAgreement;

public class AcknowledgeEmployerAgreementCommand : IRequest
{
    public long AgreementId { get; private set; }

    public AcknowledgeEmployerAgreementCommand(long agreementId)
    {
        AgreementId = agreementId;
    }
}