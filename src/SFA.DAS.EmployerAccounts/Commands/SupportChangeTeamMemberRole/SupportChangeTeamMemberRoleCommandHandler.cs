using System.Threading;
using SFA.DAS.EmployerAccounts.Audit.Types;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Types.Models;
using SFA.DAS.Encoding;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerAccounts.Commands.SupportChangeTeamMemberRole;

public class SupportChangeTeamMemberRoleCommandHandler : IRequestHandler<SupportChangeTeamMemberRoleCommand>
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly SupportChangeTeamMemberRoleCommandValidator _validator;
    private readonly IEncodingService _encodingService;
    private readonly IEmployerAccountRepository _employerAccountRepository;
    private readonly IAuditService _auditService;

    public SupportChangeTeamMemberRoleCommandHandler(
        IMembershipRepository membershipRepository, 
        IEventPublisher eventPublisher,
        IEncodingService encodingService, 
        IEmployerAccountRepository employerAccountRepository,
        IAuditService auditService)
    {
        _membershipRepository = membershipRepository ?? throw new ArgumentNullException(nameof(membershipRepository));
        _eventPublisher = eventPublisher;
        _encodingService = encodingService;
        _employerAccountRepository = employerAccountRepository;
        _auditService = auditService;
        _validator = new SupportChangeTeamMemberRoleCommandValidator();
    }

    public async Task Handle(SupportChangeTeamMemberRoleCommand message, CancellationToken cancellationToken)
    {
        var validationResult = _validator.Validate(message);

        if (!validationResult.IsValid())
        {
            throw new InvalidRequestException(validationResult.ValidationDictionary);
        }

        var accountId = _encodingService.Decode(message.HashedAccountId, EncodingType.AccountId);
        var existing = await _membershipRepository.Get(accountId, message.Email);

        if (existing == null)
        {
            throw new InvalidRequestException(new Dictionary<string, string> { { "Membership", "Membership not found" } });
        }

        var accountOwner = await GetAccountOwner(accountId);
        
        await AddAuditEntry(message, accountId, existing, accountOwner.Email);
        
        await _membershipRepository.ChangeRole(existing.Id, accountId, message.Role);

        await _eventPublisher.Publish(new AccountUserRolesUpdatedEvent(accountId, existing.UserRef, (UserRole)message.Role, DateTime.UtcNow));
    }

    private async Task AddAuditEntry(SupportChangeTeamMemberRoleCommand message, long accountId, TeamMember existing, string accountOwnerEmail)
    {
        var auditMessage = new AuditMessage
        {
            ImpersonatedUserEmail = accountOwnerEmail,
            Category = "UPDATED",
            Description = $"Member {message.Email} on account {accountId} role has changed to {message.Role}",
            ChangedProperties = new List<PropertyUpdate>
            {
                new PropertyUpdate {PropertyName = "Role",NewValue = message.Role.ToString()}
            },
            RelatedEntities = new List<AuditEntity> { new AuditEntity { Id = accountId.ToString(), Type = "Account" } },
            AffectedEntity = new AuditEntity { Type = "Membership", Id = existing.Id.ToString() }
        };

        await _auditService.SendAuditMessage(auditMessage);
    }

    private async Task<User> GetAccountOwner(long accountId)
    {
        var account = await _employerAccountRepository.GetAccountById(accountId);
        return account.Memberships.First(x => x.Role == Role.Owner).User;
    }
}