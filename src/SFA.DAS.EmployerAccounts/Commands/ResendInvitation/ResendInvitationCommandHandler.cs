using System.Threading;
using SFA.DAS.EmployerAccounts.Audit.Types;
using SFA.DAS.EmployerAccounts.Commands.AuditCommand;
using SFA.DAS.EmployerAccounts.Commands.SendNotification;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.Encoding;
using SFA.DAS.TimeProvider;

namespace SFA.DAS.EmployerAccounts.Commands.ResendInvitation;

public class ResendInvitationCommandHandler : IRequestHandler<ResendInvitationCommand>
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IMediator _mediator;
    private readonly EmployerAccountsConfiguration _employerApprenticeshipsServiceConfiguration;
    private readonly IUserAccountRepository _userRepository;
    private readonly ResendInvitationCommandValidator _validator;
    private readonly IEncodingService _encodingService;

    public ResendInvitationCommandHandler(IInvitationRepository invitationRepository,
        IMembershipRepository membershipRepository, 
        IMediator mediator,
        EmployerAccountsConfiguration employerApprenticeshipsServiceConfiguration,
        IUserAccountRepository userRepository, 
        IEncodingService encodingService)
    {
        ArgumentNullException.ThrowIfNull(invitationRepository);
        ArgumentNullException.ThrowIfNull(membershipRepository);
        ArgumentNullException.ThrowIfNull(mediator);
        ArgumentNullException.ThrowIfNull(employerApprenticeshipsServiceConfiguration);
        
        _invitationRepository = invitationRepository;
        _membershipRepository = membershipRepository;
        _mediator = mediator;
        _employerApprenticeshipsServiceConfiguration = employerApprenticeshipsServiceConfiguration;
        _userRepository = userRepository;
        _encodingService = encodingService;
        _validator = new ResendInvitationCommandValidator();
    }

    public async Task Handle(ResendInvitationCommand message, CancellationToken cancellationToken)
    {
        var validationResult = _validator.Validate(message);

        if (!validationResult.IsValid())
        {
            throw new InvalidRequestException(validationResult.ValidationDictionary);
        }

        var owner = await _membershipRepository.GetCaller(message.HashedAccountId, message.ExternalUserId);

        if (owner == null || owner.Role != Role.Owner)
        {
            throw new InvalidRequestException(new Dictionary<string, string> { { "Membership", "User is not an Owner" } });
        }

        var invitationId = _encodingService.Decode(message.HashedInvitationId, EncodingType.AccountId);

        var invitation = await _invitationRepository.Get(owner.AccountId, invitationId);

        if (invitation == null)
        {
            throw new InvalidRequestException(new Dictionary<string, string> { { "Invitation", "Invitation not found" } });
        }

        if (invitation.Status == InvitationStatus.Accepted)
        {
            throw new InvalidRequestException(new Dictionary<string, string> { { "Invitation", "Accepted invitations cannot be resent" } });
        }

        invitation.Status = InvitationStatus.Pending;
        var expiryDate = DateTimeProvider.Current.UtcNow.Date.AddDays(8);
        invitation.ExpiryDate = expiryDate;

        await _invitationRepository.Resend(invitation);

        var existingUser = await _userRepository.Get(invitation.Email);

        await AddAuditEntry(invitation, cancellationToken);

        await SendNotification(message, existingUser, owner, expiryDate, cancellationToken);
    }

    private async Task AddAuditEntry(Invitation invitation, CancellationToken cancellationToken)
    {
        await _mediator.Send(new CreateAuditCommand
        {
            EasAuditMessage = new AuditMessage
            {
                Category = "INVITATION_RESENT",
                Description = $"Invitation to {invitation.Email} resent in Account {invitation.AccountId}",
                ChangedProperties =
                [
                    new() { PropertyName = "Status", NewValue = invitation.Status.ToString() },
                    new() { PropertyName = "ExpiryDate", NewValue = invitation.ExpiryDate.ToString() }
                ],
                RelatedEntities = [new() { Id = invitation.AccountId.ToString(), Type = "Account" }],
                AffectedEntity = new AuditEntity { Type = "Invitation", Id = invitation.Id.ToString() }
            }
        }, cancellationToken);
    }

    private async Task SendNotification(ResendInvitationCommand message, User existingUser, MembershipView owner, DateTime expiryDate, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SendNotificationCommand
        {
            RecipientsAddress = existingUser.Email,
            TemplateId = existingUser?.UserRef != null ? "InvitationExistingUser" : "InvitationNewUser",
            Tokens = new Dictionary<string, string>
            {
                { "account_name", owner.AccountName },
                { "first_name", message.FirstName },
                { "inviter_name", $"{owner.FirstName} {owner.LastName}" },
                { "base_url", _employerApprenticeshipsServiceConfiguration.DashboardUrl },
                { "expiry_date", expiryDate.ToString("dd MMM yyy") }
            }
        }, cancellationToken);
    }
}