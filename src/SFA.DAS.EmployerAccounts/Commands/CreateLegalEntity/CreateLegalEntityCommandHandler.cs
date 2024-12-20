﻿using System.Threading;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Audit.Types;
using SFA.DAS.EmployerAccounts.Commands.AuditCommand;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Extensions;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;
using SFA.DAS.Encoding;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerAccounts.Commands.CreateLegalEntity;

public class CreateLegalEntityCommandHandler(
    IAccountRepository accountRepository,
    IMembershipRepository membershipRepository,
    IMediator mediator,
    IEventPublisher eventPublisher,
    IEncodingService encodingService,
    IEmployerAgreementRepository employerAgreementRepository,
    IValidator<CreateLegalEntityCommand> validator)
    : IRequestHandler<CreateLegalEntityCommand, CreateLegalEntityCommandResponse>
{
    public async Task<CreateLegalEntityCommandResponse> Handle(CreateLegalEntityCommand message, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(message);

        if (!validationResult.IsValid())
        {
            throw new InvalidRequestException(validationResult.ValidationDictionary);
        }

        if (validationResult.IsUnauthorized)
        {
            throw new UnauthorizedAccessException();
        }

        var owner = await membershipRepository.GetCaller(message.HashedAccountId, message.ExternalUserId);

        var ownerExternalUserId = owner.UserRef;

        var createParams = new CreateLegalEntityWithAgreementParams
        {
            AccountId = owner.AccountId,
            Name = message.Name,
            Status = message.Status,
            Code = string.IsNullOrEmpty(message.Code) ? Guid.NewGuid().ToString() : message.Code,
            DateOfIncorporation = message.DateOfIncorporation,
            PublicSectorDataSource = message.PublicSectorDataSource,
            Source = message.Source,
            Address = message.Address,
            Sector = message.Sector,
            AgreementType = AgreementType.Combined
        };

        var agreementView = await accountRepository.CreateLegalEntityWithAgreement(createParams);

        var accountId = encodingService.Decode(message.HashedAccountId, EncodingType.AccountId);
        agreementView.AccountLegalEntityPublicHashedId = encodingService.Encode(agreementView.AccountLegalEntityId, EncodingType.PublicAccountLegalEntityId);
        agreementView.HashedAgreementId = encodingService.Encode(agreementView.Id, EncodingType.AccountId);

        await Task.WhenAll(
            CreateAuditEntries(owner, agreementView),
            SetEmployerLegalEntityAgreementStatus(agreementView.AccountLegalEntityId, agreementView.Id, agreementView.VersionNumber),
            PublishLegalEntityAddedMessage(accountId, agreementView.Id, createParams.Name, owner.FullName(), agreementView.LegalEntityId,
                agreementView.AccountLegalEntityId, agreementView.AccountLegalEntityPublicHashedId, createParams.Code, message.Address, message.Source, ownerExternalUserId),
            PublishAgreementCreatedMessage(accountId, agreementView.Id, createParams.Name, owner.FullName(), agreementView.LegalEntityId, ownerExternalUserId)
        );

        return new CreateLegalEntityCommandResponse
        {
            AgreementView = agreementView
        };
    }

    private Task PublishLegalEntityAddedMessage(long accountId, long agreementId, string organisationName, string createdByName, long legalEntityId, long accountLegalEntityId, string accountLegalEntityPublicHashedId, string organisationReferenceNumber, string organisationAddress, OrganisationType organisationType, Guid userRef)
    {
        return eventPublisher.Publish(new AddedLegalEntityEvent
        {
            AccountId = accountId,
            AgreementId = agreementId,
            LegalEntityId = legalEntityId,
            AccountLegalEntityId = accountLegalEntityId,
            AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId,
            OrganisationName = organisationName,
            UserName = createdByName,
            UserRef = userRef,
            Created = DateTime.UtcNow,
            OrganisationReferenceNumber = organisationReferenceNumber,
            OrganisationAddress = organisationAddress,
            OrganisationType = (SFA.DAS.EmployerAccounts.Types.Models.OrganisationType) organisationType
        });
    }

    private Task PublishAgreementCreatedMessage(long accountId, long agreementId, string organisationName, string createdByName, long legalEntityId, Guid userRef)
    {
        return eventPublisher.Publish(new CreatedAgreementEvent
        {
            AgreementId = agreementId,
            LegalEntityId = legalEntityId,
            OrganisationName = organisationName,
            AccountId = accountId,
            UserName = createdByName,
            UserRef = userRef,
            Created = DateTime.UtcNow
        });
    }
    
    private Task SetEmployerLegalEntityAgreementStatus(long accountLegalEntityId, long agreementId, int agreementVersion)
    {
        return employerAgreementRepository.SetAccountLegalEntityAgreementDetails(accountLegalEntityId, agreementId, agreementVersion, null, null);
    }

    private async Task CreateAuditEntries(MembershipView owner, EmployerAgreementView agreementView)
    {
        await mediator.Send(new CreateAuditCommand
        {
            EasAuditMessage = new AuditMessage
            {
                Category = "UPDATED",
                Description = $"User {owner.Email} added legal entity {agreementView.LegalEntityId} to account {owner.AccountId}",
                ChangedProperties = new List<PropertyUpdate>
                {
                    new PropertyUpdate { PropertyName = "AccountId", NewValue = agreementView.AccountId.ToString() },
                    new PropertyUpdate { PropertyName = "Id", NewValue = agreementView.LegalEntityId.ToString() },
                    new PropertyUpdate { PropertyName = "Name", NewValue = agreementView.LegalEntityName },
                    new PropertyUpdate { PropertyName = "Code", NewValue = agreementView.LegalEntityCode },
                    new PropertyUpdate { PropertyName = "Source", NewValue = agreementView.LegalEntitySource.ToString() },
                    new PropertyUpdate { PropertyName = "Status", NewValue = agreementView.LegalEntityStatus },
                    new PropertyUpdate { PropertyName = "Address", NewValue = agreementView.LegalEntityAddress },
                    new PropertyUpdate { PropertyName = "DateOfInception", NewValue = agreementView.LegalEntityInceptionDate.GetDateString("G") },
                },
                RelatedEntities = new List<AuditEntity> { new AuditEntity { Id = agreementView.AccountId.ToString(), Type = "Account" } },
                AffectedEntity = new AuditEntity { Type = "LegalEntity", Id = agreementView.LegalEntityId.ToString() }
            }
        });

        await mediator.Send(new CreateAuditCommand
        {
            EasAuditMessage = new AuditMessage
            {
                Category = "UPDATED",
                Description = $"User {owner.Email} added signed agreement {agreementView.Id} to account {owner.AccountId}",
                ChangedProperties = new List<PropertyUpdate>
                {
                    new PropertyUpdate { PropertyName = "AccountId", NewValue = agreementView.AccountId.ToString() },
                    new PropertyUpdate { PropertyName = "SignedDate", NewValue = agreementView.SignedDate.GetDateString("G") },
                    new PropertyUpdate { PropertyName = "SignedBy", NewValue = $"{owner.FirstName} {owner.LastName}" }
                },
                RelatedEntities = new List<AuditEntity> { new AuditEntity { Id = agreementView.AccountId.ToString(), Type = "Account" } },
                AffectedEntity = new AuditEntity { Type = "EmployerAgreement", Id = agreementView.Id.ToString() }
            }
        });
    }
}