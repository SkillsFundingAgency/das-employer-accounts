using System.Threading;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;
using SFA.DAS.Encoding;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerAccounts.Commands.SignEmployerAgreementWithOutAudit;

public class SignEmployerAgreementWithoutAuditCommandHandler(
    IEncodingService _encodingService,
    IEmployerAgreementRepository _employerAgreementRepository,
    IEventPublisher _eventPublisher,
    IValidator<SignEmployerAgreementWithoutAuditCommand> _validator)
    : IRequestHandler<SignEmployerAgreementWithoutAuditCommand>
{
    public async Task Handle(SignEmployerAgreementWithoutAuditCommand request, CancellationToken cancellationToken)
    {
        await ValidateRequest(request);

        var agreementId = _encodingService.Decode(request.HashedAgreementId, EncodingType.AccountId);

        var signedAgreementDetails = new Models.EmployerAgreement.SignEmployerAgreement
        {
            SignedDate = DateTime.UtcNow,
            AgreementId = agreementId,
            SignedById = request.User.Id,
            SignedByName = request.User.FullName
        };

        await _employerAgreementRepository.SignAgreement(signedAgreementDetails);

        EmployerAgreementView agreement = await _employerAgreementRepository.GetEmployerAgreement(agreementId);

        await _eventPublisher.Publish(new SignedAgreementEvent
        {
            AgreementId = agreementId,
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
