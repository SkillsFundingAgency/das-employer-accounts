using System.Threading;
using SFA.DAS.EmployerAccounts.Data.Contracts;

namespace SFA.DAS.EmployerAccounts.Commands.AcknowledgeEmployerAgreement;

public class AcknowledgeEmployerAgreementCommandHandler : IRequestHandler<AcknowledgeEmployerAgreementCommand>
{
    private readonly IEmployerAgreementRepository _employerAgreementRepository;

    public AcknowledgeEmployerAgreementCommandHandler(IEmployerAgreementRepository employerAgreementRepository)
    {
        _employerAgreementRepository = employerAgreementRepository;
    }

    public async Task Handle(AcknowledgeEmployerAgreementCommand message, CancellationToken cancellationToken)
    {
        await _employerAgreementRepository.AcknowledgeEmployerAgreement(message.AgreementId);
    }
}