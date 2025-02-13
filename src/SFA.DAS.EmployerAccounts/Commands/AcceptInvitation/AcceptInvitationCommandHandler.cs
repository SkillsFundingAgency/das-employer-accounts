using System.Threading;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerAccounts.Audit.Types;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Types.Models;
using SFA.DAS.Encoding;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerAccounts.Commands.AcceptInvitation;

public class AcceptInvitationCommandHandler : IRequestHandler<AcceptInvitationCommand>
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IAuditService _auditService;
    private readonly IValidator<AcceptInvitationCommand> _validator;
    private readonly IEncodingService _encodingService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<AcceptInvitationCommandHandler> _logger;

    public AcceptInvitationCommandHandler(IInvitationRepository invitationRepository,
        IMembershipRepository membershipRepository,
        IUserAccountRepository userAccountRepository,
        IAuditService auditService,
        IEventPublisher eventPublisher,
        IValidator<AcceptInvitationCommand> validator,
        IEncodingService encodingService,
        ILogger<AcceptInvitationCommandHandler> logger)
    {
        _invitationRepository = invitationRepository;
        _membershipRepository = membershipRepository;
        _userAccountRepository = userAccountRepository;
        _auditService = auditService;
        _eventPublisher = eventPublisher;
        _validator = validator;
        _encodingService = encodingService;
        _logger = logger;
    }

    public async Task Handle(AcceptInvitationCommand message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Accepting Invitation '{Id}'", message.Id);

        var validationResult = _validator.Validate(message);

        if (!validationResult.IsValid())
        {
            throw new InvalidRequestException(validationResult.ValidationDictionary);
        }

        var invitation = await GetInvitation(message);

        var user = await GetUser(invitation.Email);

        await CheckIfUserIsAlreadyAMember(invitation, user);

        if (invitation.Status != InvitationStatus.Pending)
        {
            throw new InvalidOperationException("Invitation is not pending");
        }

        if (invitation.ExpiryDate < TimeProvider.System.GetUtcNow())
        {
            throw new InvalidOperationException("Invitation has expired");
        }

        await _invitationRepository.Accept(invitation.Email, invitation.AccountId, invitation.Role);

        await CreateAuditEntry(message, user, invitation);

        await PublishUserJoinedMessage(invitation.AccountId, user, invitation);
    }

    private async Task CheckIfUserIsAlreadyAMember(Invitation invitation, User user)
    {
        var membership = await _membershipRepository.GetCaller(invitation.AccountId, user.UserRef);

        if (membership != null)
        {
            throw new InvalidOperationException("Invited user is already a member of the Account");
        }
    }

    private async Task<User> GetUser(string email)
    {
        var user = await _userAccountRepository.Get(email);

        if (user == null)
        {
            throw new InvalidOperationException("Invited user was not found");
        }

        return user;
    }

    private async Task<Invitation> GetInvitation(AcceptInvitationCommand message)
    {
        var invitation = await _invitationRepository.Get(message.Id);

        if (invitation == null)
        {
            throw new InvalidRequestException(new Dictionary<string, string> { { "Id", "Invitation not found with given ID" } });
        }

        return invitation;
    }

    private async Task CreateAuditEntry(AcceptInvitationCommand message, User user, Invitation existing)
    {
        await _auditService.SendAuditMessage(new AuditMessage
        {
            Category = "UPDATED",
            Description =
                $"Member {user.Email} has accepted and invitation to account {existing.AccountId} as {existing.Role}",
            ChangedProperties = new List<PropertyUpdate>
            {
                PropertyUpdate.FromString("Status", InvitationStatus.Accepted.ToString())
            },
            RelatedEntities = new List<AuditEntity>
            {
                new AuditEntity {Id = $"Account Id [{existing.AccountId}], User Id [{user.Id}]", Type = "Membership"}
            },
            AffectedEntity = new AuditEntity { Type = "Invitation", Id = message.Id.ToString() }
        });
    }

    private Task PublishUserJoinedMessage(long accountId, User user, Invitation invitation)
    {
        return _eventPublisher.Publish(new UserJoinedEvent
        {
            AccountId = accountId,
            HashedAccountId = _encodingService.Encode(accountId, EncodingType.AccountId),
            UserName = user.FullName,
            UserRef = user.Ref,
            Role = (UserRole)invitation.Role,
            Created = DateTime.UtcNow
        });
    }
}