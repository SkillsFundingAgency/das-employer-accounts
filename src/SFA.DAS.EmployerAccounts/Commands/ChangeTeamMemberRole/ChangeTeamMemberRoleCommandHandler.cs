using System.Threading;
using SFA.DAS.EmployerAccounts.Audit.Types;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Types.Models;
using SFA.DAS.Encoding;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerAccounts.Commands.ChangeTeamMemberRole;

public class ChangeTeamMemberRoleCommandHandler : IRequestHandler<ChangeTeamMemberRoleCommand>
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IEncodingService _encodingService;
    private readonly ChangeTeamMemberRoleCommandValidator _validator;
    private readonly IAuditService _auditService;

    public ChangeTeamMemberRoleCommandHandler(IMembershipRepository membershipRepository, IEventPublisher eventPublisher, IEncodingService encodingService, IAuditService auditService)
    {
        _membershipRepository = membershipRepository;
        _eventPublisher = eventPublisher;
        _encodingService = encodingService;
        _auditService = auditService;
        _validator = new ChangeTeamMemberRoleCommandValidator();
    }

    public async Task Handle(ChangeTeamMemberRoleCommand message, CancellationToken cancellationToken)
    {
        var validationResult = _validator.Validate(message);

        if (!validationResult.IsValid())
        {
            throw new InvalidRequestException(validationResult.ValidationDictionary);
        }

        var caller = await _membershipRepository.GetCaller(message.HashedAccountId, message.ExternalUserId);

        if (caller == null)
        {
            throw new InvalidRequestException(new Dictionary<string, string> { { "Membership", "You are not a member of this Account" } });
        }

        if (caller.Role != Role.Owner)
        {
            throw new InvalidRequestException(new Dictionary<string, string> { { "Membership", "You must be an owner of this Account" } });
        }

        var userId = _encodingService.Decode(message.HashedUserId, EncodingType.AccountId);
        var existing = await _membershipRepository.Get(userId, caller.AccountId);

        if (existing == null)
        {
            throw new InvalidRequestException(new Dictionary<string, string> { { "Membership", "Membership not found" } });
        }

        if (caller.UserId == existing.Id)
        {
            throw new InvalidRequestException(new Dictionary<string, string> { { "Membership", "You cannot change your own role" } });
        }

        await _membershipRepository.ChangeRole(existing.Id, caller.AccountId, message.Role);

        await _eventPublisher.Publish(new AccountUserRolesUpdatedEvent(caller.AccountId, existing.UserRef, (UserRole)message.Role, DateTime.UtcNow));

        await AddAuditEntry(message, cancellationToken, existing, caller);
    }

    private async Task AddAuditEntry(ChangeTeamMemberRoleCommand message, CancellationToken cancellationToken, TeamMember existing, MembershipView caller)
    {
        var auditMessage = new AuditMessage
        {
            Category = "UPDATED",
            Description = $"Member {existing.Email} on account {caller.AccountId} role has changed to {message.Role}",
            ChangedProperties = [new() { PropertyName = "Role", NewValue = message.Role.ToString() }],
            RelatedEntities = [new() { Id = caller.AccountId.ToString(), Type = "Account" }],
            AffectedEntity = new AuditEntity { Type = "Membership", Id = existing.Id.ToString() }
        };

        await _auditService.SendAuditMessage(auditMessage);
    }
}