using System.Threading;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerAccounts.Audit.Types;
using SFA.DAS.EmployerAccounts.Commands.AuditCommand;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;
using SFA.DAS.Encoding;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerAccounts.Commands.RemoveLegalEntity;

public class RemoveLegalEntityCommandHandler(
    IValidator<RemoveLegalEntityCommand> validator,
    ILogger<RemoveLegalEntityCommandHandler> logger,
    IEmployerAgreementRepository employerAgreementRepository,
    IMediator mediator,
    IEncodingService encodingService,
    IMembershipRepository membershipRepository,
    IEventPublisher eventPublisher,
    ICommitmentsV2ApiClient commitmentsV2ApiClient)
    : IRequestHandler<RemoveLegalEntityCommand>
{
    public async Task Handle(RemoveLegalEntityCommand message, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(message);

        if (!validationResult.IsValid())
        {
            throw new InvalidRequestException(validationResult.ValidationDictionary);
        }

        if (validationResult.IsUnauthorized)
        {
            logger.LogInformation("User {UserId} tried to remove {AccountLegalEntityId} from Account {AccountId}", message.UserId, message.AccountLegalEntityId, message.AccountId);
            throw new UnauthorizedAccessException();
        }

        var agreements = await employerAgreementRepository.GetAccountLegalEntityAgreements(message.AccountLegalEntityId);
        var legalAgreement = agreements.OrderByDescending(a => a.TemplateId).First();

        var hashedAccountId = encodingService.Encode(message.AccountId, EncodingType.AccountId);
        var hashedLegalAgreementId = encodingService.Encode(legalAgreement.Id, EncodingType.AccountId);

        var agreement = await employerAgreementRepository.GetEmployerAgreement(legalAgreement.Id);

        if (agreements.Any(x => x.SignedDate.HasValue))
        {
            await ValidateLegalEntityHasNoCommitments(agreement, message.AccountId, validationResult);
        }

        await employerAgreementRepository.RemoveLegalEntityFromAccount(legalAgreement.Id);

        await AddAuditEntry(hashedAccountId, hashedLegalAgreementId);
        
        // it appears that an agreement is created whenever we create a legal entity, so there should always be an agreement associated with a legal entity
        if (agreement == null)
        {
            return;
        }

        var agreementSigned = agreement.Status == EmployerAgreementStatus.Signed;
        var caller = await membershipRepository.GetCaller(message.AccountId, message.UserId);
        var createdByName = caller.FullName();

        await PublishLegalEntityRemovedMessage(
            message.AccountId,
            legalAgreement.Id,
            agreementSigned,
            createdByName,
            agreement.LegalEntityId,
            agreement.LegalEntityName,
            agreement.AccountLegalEntityId,
            message.UserId);
    }

    private async Task ValidateLegalEntityHasNoCommitments(EmployerAgreementView agreement, long accountId, ValidationResult validationResult)
    {
        var response = await commitmentsV2ApiClient.GetEmployerAccountSummary(accountId);
        
        var commitment = response.ApprenticeshipStatusSummaryResponse.FirstOrDefault(c =>
            !string.IsNullOrEmpty(c.LegalEntityIdentifier)
            && c.LegalEntityIdentifier.Equals(agreement.LegalEntityCode)
            && c.LegalEntityOrganisationType == agreement.LegalEntitySource);
        
        if (commitment != null && commitment.ActiveCount + commitment.CompletedCount + commitment.PausedCount + commitment.PendingApprovalCount + commitment.WithdrawnCount != 0)
        {
            validationResult.AddError(nameof(agreement.Id), "Agreement has already been signed and has active commitments");
            throw new InvalidRequestException(validationResult.ValidationDictionary);
        }
    }

    private Task PublishLegalEntityRemovedMessage(
        long accountId, long agreementId, bool agreementSigned, string createdBy,
        long legalEntityId, string organisationName, long accountLegalEntityId, string userRef)
    {
        return eventPublisher.Publish(new RemovedLegalEntityEvent
        {
            AccountId = accountId,
            AgreementId = agreementId,
            LegalEntityId = legalEntityId,
            AgreementSigned = agreementSigned,
            OrganisationName = organisationName,
            AccountLegalEntityId = accountLegalEntityId,
            Created = DateTime.UtcNow,
            UserName = createdBy,
            UserRef = Guid.Parse(userRef)
        });
    }

    private async Task AddAuditEntry(string hashedAccountId, string employerAgreementId)
    {
        await mediator.Send(new CreateAuditCommand
        {
            EasAuditMessage = new AuditMessage
            {
                Category = "UPDATED",
                Description = $"EmployerAgreement {employerAgreementId} removed from account {hashedAccountId}",
                ChangedProperties = new List<PropertyUpdate>
                {
                    PropertyUpdate.FromString("Status", EmployerAgreementStatus.Removed.ToString())
                },
                RelatedEntities = new List<AuditEntity> { new() { Id = hashedAccountId, Type = "Account" } },
                AffectedEntity = new AuditEntity { Type = "EmployerAgreement", Id = employerAgreementId }
            }
        });
    }
}