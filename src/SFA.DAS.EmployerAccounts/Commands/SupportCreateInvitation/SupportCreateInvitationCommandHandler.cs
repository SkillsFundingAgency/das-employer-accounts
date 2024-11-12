using System.Threading;
using NServiceBus;
using SFA.DAS.EmployerAccounts.Audit.Types;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.Encoding;
using SFA.DAS.Notifications.Messages.Commands;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerAccounts.Commands.SupportCreateInvitation;

public class SupportCreateInvitationCommandHandler(
    IValidator<SupportCreateInvitationCommand> validator,
    IInvitationRepository invitationRepository,
    IAuditService auditService,
    IEncodingService encodingService,
    EmployerAccountsConfiguration employerAccountsConfiguration,
    IEventPublisher eventPublisher,
    IUserAccountRepository userAccountRepository,
    IEmployerAccountRepository employerAccountRepository,
    IMessageSession publisher,
    TimeProvider timeProvider)
    : IRequestHandler<SupportCreateInvitationCommand>
{

    public async Task Handle(SupportCreateInvitationCommand message, CancellationToken cancellationToken)
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

        var accountId = encodingService.Decode(message.HashedAccountId, EncodingType.AccountId);

        // Verify the email is not used by an existing invitation for the account
        var existingInvitation = await invitationRepository.Get(accountId, message.EmailOfPersonBeingInvited);

        if (existingInvitation != null && existingInvitation.Status != InvitationStatus.Deleted && existingInvitation.Status != InvitationStatus.Accepted)
            throw new InvalidRequestException(new Dictionary<string, string> { { "ExistingMember", $"{message.EmailOfPersonBeingInvited} is already invited" } });

        var expiryDate = timeProvider.GetUtcNow().Date.AddDays(8);

        var invitationId = 0L;

        if (existingInvitation == null)
        {
            invitationId = await invitationRepository.Create(new Invitation
            {
                AccountId = accountId,
                Email = message.EmailOfPersonBeingInvited,
                Name = message.NameOfPersonBeingInvited,
                Role = message.RoleOfPersonBeingInvited,
                Status = InvitationStatus.Pending,
                ExpiryDate = expiryDate
            });
        }
        else
        {
            existingInvitation.Name = message.NameOfPersonBeingInvited;
            existingInvitation.Role = message.RoleOfPersonBeingInvited;
            existingInvitation.Status = InvitationStatus.Pending;
            existingInvitation.ExpiryDate = expiryDate;

            await invitationRepository.Resend(existingInvitation);

            invitationId = existingInvitation.Id;
        }

        var accountOwner = await GetAccountOwner(accountId);
        
        await AddAuditEntry(message, accountId, expiryDate, invitationId, accountOwner.Email);

        await SendInvitation(message, expiryDate, accountId);
        
        await PublishUserInvitedEvent(accountId, message.NameOfPersonBeingInvited, accountOwner.Email, accountOwner.Ref);
    }
    
    private async Task<User> GetAccountOwner(long accountId)
    {
        var account = await employerAccountRepository.GetAccountById(accountId);
        return account.Memberships.First(x => x.Role == Role.Owner).User;
    }

    private async Task SendInvitation(SupportCreateInvitationCommand message, DateTime expiryDate, long accountId)
    {
        var existingUser = await userAccountRepository.Get(message.EmailOfPersonBeingInvited);
        var account = await employerAccountRepository.GetAccountById(accountId);

        var tokens = new Dictionary<string, string>
        {
            { "account_name", account.Name },
            { "first_name", existingUser != null ? existingUser.FirstName : message.NameOfPersonBeingInvited },
            { "inviter_name", "Apprenticeship Service Support" },
            { "base_url", employerAccountsConfiguration.DashboardUrl },
            { "expiry_date", expiryDate.ToString("dd MMM yyy") }
        };
        
        var templateId = existingUser?.Ref != null ? "InvitationExistingUser" : "InvitationNewUser";
        
        await publisher.Send(new SendEmailCommand(templateId, message.EmailOfPersonBeingInvited, tokens));
    }

    private async Task AddAuditEntry(SupportCreateInvitationCommand message, long accountId, DateTime expiryDate, long invitationId, string accountOwnerEmail)
    {
        await auditService.SendAuditMessage(new AuditMessage
        {
            ImpersonatedUserEmail = accountOwnerEmail,
            Category = "CREATED",
            Description = $"Member {message.EmailOfPersonBeingInvited} added to account {accountId} as {message.RoleOfPersonBeingInvited}",
            ChangedProperties = new List<PropertyUpdate>
            {
                PropertyUpdate.FromString("AccountId", accountId.ToString()),
                PropertyUpdate.FromString("Email", message.EmailOfPersonBeingInvited),
                PropertyUpdate.FromString("Name", message.NameOfPersonBeingInvited),
                PropertyUpdate.FromString("Role", message.RoleOfPersonBeingInvited.ToString()),
                PropertyUpdate.FromString("Status", InvitationStatus.Pending.ToString()),
                PropertyUpdate.FromDateTime("ExpiryDate", expiryDate)
            },
            RelatedEntities = new List<AuditEntity> { new AuditEntity { Id = accountId.ToString(), Type = "Account" } },
            AffectedEntity = new AuditEntity { Type = "Invitation", Id = invitationId.ToString() }
        });
    }

    private Task PublishUserInvitedEvent(long accountId, string personInvited, string invitedByUserName, Guid invitedByUserRef)
    {
        return eventPublisher.Publish(new InvitedUserEvent
        {
            AccountId = accountId,
            PersonInvited = personInvited,
            UserName = invitedByUserName,
            UserRef = invitedByUserRef,
            Created = DateTime.UtcNow
        });
    }
}