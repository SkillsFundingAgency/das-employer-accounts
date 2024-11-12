using System.Threading;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerAccounts.Commands.SignEmployerAgreementWithOutAudit;

public class SignEmployerAgreementWithoutAuditCommandHandler(
    IEmployerAgreementRepository _employerAgreementRepository,
    IEventPublisher _eventPublisher,
    IValidator<SignEmployerAgreementWithoutAuditCommand> _validator)
    : IRequestHandler<SignEmployerAgreementWithoutAuditCommand>
{
    public async Task Handle(SignEmployerAgreementWithoutAuditCommand request, CancellationToken cancellationToken)
    {
        await ValidateRequest(request);

        var signedAgreementDetails = new Models.EmployerAgreement.SignEmployerAgreement
        {
            SignedDate = DateTime.UtcNow,
            AgreementId = request.AgreementId,
            SignedById = request.User.Id,
            SignedByName = request.User.FullName
        };

        await _employerAgreementRepository.SignAgreement(signedAgreementDetails);

        EmployerAgreementView agreement = await _employerAgreementRepository.GetEmployerAgreement(request.AgreementId);

        await _employerAgreementRepository.SetAccountLegalEntityAgreementDetails(agreement.AccountLegalEntityId, null, null, agreement.Id, agreement.VersionNumber, false);

        await _eventPublisher.Publish(new SignedAgreementEvent
        {
            AgreementId = request.AgreementId,
            AccountId = agreement.AccountId,
            AccountLegalEntityId = agreement.AccountLegalEntityId,
            LegalEntityId = agreement.LegalEntityId,
            OrganisationName = agreement.LegalEntityName,
            CohortCreated = false,
            Created = DateTime.UtcNow,
            UserName = request.User.FullName,
            UserRef = request.User.Ref,
            AgreementType = agreement.AgreementType,
            SignedAgreementVersion = agreement.VersionNumber,
            CorrelationId = request.CorrelationId
        });
    }

    private async Task ValidateRequest(SignEmployerAgreementWithoutAuditCommand message)
    {
        var validationResult = await _validator.ValidateAsync(message);

        if (!validationResult.IsValid())
            throw new InvalidRequestException(validationResult.ValidationDictionary);
    }
}
