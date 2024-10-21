using System.Threading;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Audit.Types;
using SFA.DAS.EmployerAccounts.Commands.AuditCommand;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;
using SFA.DAS.EmployerAccounts.Queries.GetUserByRef;
using SFA.DAS.Encoding;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerAccounts.Commands.SignEmployerAgreement;

public class SignEmployerAgreementCommandHandler(
    IMembershipRepository membershipRepository,
    IEmployerAgreementRepository employerAgreementRepository,
    IEncodingService encodingService,
    IValidator<SignEmployerAgreementCommand> validator,
    IMediator mediator,
    IEventPublisher eventPublisher,
    ICommitmentV2Service commitmentService)
    : IRequestHandler<SignEmployerAgreementCommand, SignEmployerAgreementCommandResponse>
{
    public async Task<SignEmployerAgreementCommandResponse> Handle(SignEmployerAgreementCommand message, CancellationToken cancellationToken)
    {
        await ValidateRequest(message);
        var owner = await VerifyUserIsAccountOwner(message);
        var userResponse = await mediator.Send(new GetUserByRefQuery { UserRef = message.ExternalUserId }, cancellationToken);

        var agreementId = encodingService.Decode(message.HashedAgreementId, EncodingType.AccountId);

        await SignAgreement(message, agreementId, owner);

        var agreement = await employerAgreementRepository.GetEmployerAgreement(agreementId);

        await Task.WhenAll(
            AddAuditEntry(message, agreement.AccountId, agreementId),
            employerAgreementRepository.SetAccountLegalEntityAgreementDetails(agreement.AccountLegalEntityId, null, null, agreement.Id, agreement.VersionNumber),
            PublishAgreementSignedMessage(agreement, owner, userResponse.User.CorrelationId)
            );

        return new SignEmployerAgreementCommandResponse
        {
            AgreementType = agreement.AgreementType,
            LegalEntityName = agreement.LegalEntityName
        };
    }
    
    private async Task PublishAgreementSignedMessage(EmployerAgreementView agreement, MembershipView owner, string correlationId)
    {
        var commitments = await commitmentService.GetEmployerCommitments(agreement.AccountId);
        var accountHasCommitments = commitments?.Any() ?? false;

        await PublishAgreementSignedMessage(agreement.AccountId, agreement.AccountLegalEntityId, agreement.LegalEntityId, agreement.LegalEntityName,
            agreement.Id, accountHasCommitments, owner.FullName(), owner.UserRef, agreement.AgreementType,
            agreement.VersionNumber, correlationId);
    }

    private Task PublishAgreementSignedMessage(
        long accountId, long accountLegalEntityId, long legalEntityId, string legalEntityName, long agreementId,
        bool cohortCreated, string currentUserName, Guid currentUserRef,
        AgreementType agreementType, int versionNumber, string correlationId)
    {
        return eventPublisher.Publish(new SignedAgreementEvent
        {
            AccountId = accountId,
            AgreementId = agreementId,
            AccountLegalEntityId = accountLegalEntityId,
            LegalEntityId = legalEntityId,
            OrganisationName = legalEntityName,
            CohortCreated = cohortCreated,
            Created = DateTime.UtcNow,
            UserName = currentUserName,
            UserRef = currentUserRef,
            AgreementType = agreementType,
            SignedAgreementVersion = versionNumber,
            CorrelationId = correlationId
        });
    }
    
    private async Task<MembershipView> VerifyUserIsAccountOwner(SignEmployerAgreementCommand message)
    {
        var owner = await membershipRepository.GetCaller(message.HashedAccountId, message.ExternalUserId);

        if (owner == null || owner.Role != Role.Owner)
            throw new UnauthorizedAccessException();
        return owner;
    }

    private async Task ValidateRequest(SignEmployerAgreementCommand message)
    {
        var validationResult = await validator.ValidateAsync(message);

        if (!validationResult.IsValid())
            throw new InvalidRequestException(validationResult.ValidationDictionary);
    }

    private async Task SignAgreement(SignEmployerAgreementCommand message, long agreementId, MembershipView owner)
    {
        var signedAgreementDetails = new Models.EmployerAgreement.SignEmployerAgreement
        {
            SignedDate = message.SignedDate,
            AgreementId = agreementId,
            SignedById = owner.UserId,
            SignedByName = $"{owner.FirstName} {owner.LastName}"
        };

        await employerAgreementRepository.SignAgreement(signedAgreementDetails);
    } 

    private async Task AddAuditEntry(SignEmployerAgreementCommand message, long accountId, long agreementId)
    {
        await mediator.Send(new CreateAuditCommand
        {
            EasAuditMessage = new AuditMessage
            {
                Category = "UPDATED",
                Description = $"Agreement {agreementId} added to account {accountId}",
                ChangedProperties = new List<PropertyUpdate>
                {
                    PropertyUpdate.FromString("UserId", message.ExternalUserId),
                    PropertyUpdate.FromString("SignedDate", message.SignedDate.ToString("U"))
                },
                RelatedEntities = new List<AuditEntity> { new AuditEntity { Id = accountId.ToString(), Type = "Account" } },
                AffectedEntity = new AuditEntity { Type = "Agreement", Id = agreementId.ToString() }
            }
        });
    }
}