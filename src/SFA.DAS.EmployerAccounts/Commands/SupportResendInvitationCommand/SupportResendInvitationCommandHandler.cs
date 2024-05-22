using System.Threading;
using NServiceBus;
using SFA.DAS.EmployerAccounts.Audit.Types;
using SFA.DAS.EmployerAccounts.Commands.AuditCommand;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.Encoding;
using SFA.DAS.Notifications.Messages.Commands;
using SFA.DAS.TimeProvider;

namespace SFA.DAS.EmployerAccounts.Commands.SupportResendInvitationCommand;

public class SupportResendInvitationCommandHandler : IRequestHandler<SupportResendInvitationCommand>
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly IMediator _mediator;
    private readonly EmployerAccountsConfiguration _employerApprenticeshipsServiceConfiguration;
    private readonly IUserAccountRepository _userRepository;
    private readonly IEmployerAccountRepository _accountRepository;
    private readonly IEncodingService _encodingService;
    private readonly SupportResendInvitationCommandValidator _validator;
    private readonly IMessageSession _publisher;

    public SupportResendInvitationCommandHandler(IInvitationRepository invitationRepository,
        IMediator mediator,
        EmployerAccountsConfiguration employerApprenticeshipsServiceConfiguration,
        IUserAccountRepository userRepository,
        IEmployerAccountRepository accountRepository,
        IEncodingService encodingService, IMessageSession publisher)
    {
        _invitationRepository = invitationRepository ?? throw new ArgumentNullException(nameof(invitationRepository));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _employerApprenticeshipsServiceConfiguration = employerApprenticeshipsServiceConfiguration ?? throw new ArgumentNullException(nameof(employerApprenticeshipsServiceConfiguration));
        _userRepository = userRepository;
        _accountRepository = accountRepository;
        _encodingService = encodingService;
        _publisher = publisher;
        _validator = new SupportResendInvitationCommandValidator();
    }

    public async Task Handle(SupportResendInvitationCommand message, CancellationToken cancellationToken)
    {
        ValidateRequest(message);

        var accountId = _encodingService.Decode(message.HashedAccountId, EncodingType.AccountId);
        var account = await _accountRepository.GetAccountById(accountId);

        var existingInvitation = await _invitationRepository.Get(accountId, message.Email);

        ValidateExistingInvitation(existingInvitation);

        existingInvitation.Status = InvitationStatus.Pending;

        var expiryDate = DateTimeProvider.Current.UtcNow.Date.AddDays(8);
        existingInvitation.ExpiryDate = expiryDate;

        await _invitationRepository.Resend(existingInvitation);

        await AddAuditEntry(message, existingInvitation, cancellationToken);

        var existingUser = await _userRepository.Get(message.Email);

        await SendNotification(message, existingUser, account, expiryDate, cancellationToken);
    }

    private static void ValidateExistingInvitation(Invitation existingInvitation)
    {
        if (existingInvitation == null)
        {
            throw new InvalidRequestException(new Dictionary<string, string> { { "Invitation", "Invitation not found" } });
        }

        if (existingInvitation.Status == InvitationStatus.Accepted)
        {
            throw new InvalidRequestException(new Dictionary<string, string> { { "Invitation", "Accepted invitations cannot be resent" } });
        }
    }

    private void ValidateRequest(SupportResendInvitationCommand message)
    {
        var validationResult = _validator.Validate(message);

        if (!validationResult.IsValid())
        {
            throw new InvalidRequestException(validationResult.ValidationDictionary);
        }
    }

    private async Task SendNotification(SupportResendInvitationCommand message, User existingUser, Account account, DateTime expiryDate, CancellationToken cancellationToken)
    {
        var tokens = new Dictionary<string, string>
        {
            { "account_name", account.Name },
            { "first_name", existingUser != null ? existingUser.FirstName : message.Email },
            { "inviter_name", "Apprenticeship Service Support" },
            { "base_url", _employerApprenticeshipsServiceConfiguration.DashboardUrl },
            { "expiry_date", expiryDate.ToString("dd MMM yyy") }
        };

        var templateId = existingUser?.Ref != null ? "InvitationExistingUser" : "InvitationNewUser";
        
        await _publisher.Send(new SendEmailCommand(templateId, message.Email, tokens));
    }

    private async Task AddAuditEntry(SupportResendInvitationCommand message, Invitation existingInvitation, CancellationToken cancellationToken)
    {
        await _mediator.Send(new CreateAuditCommand
        {
            EasAuditMessage = new AuditMessage
            {
                SupportUserEmail = message.SupportUserEmail,
                Category = "INVITATION_RESENT",
                Description = $"Invitation to {message.Email} resent in Account {existingInvitation.AccountId}",
                ChangedProperties = new List<PropertyUpdate>
                {
                    new() { PropertyName = "Status", NewValue = existingInvitation.Status.ToString() },
                    new() { PropertyName = "ExpiryDate", NewValue = existingInvitation.ExpiryDate.ToString() }
                },
                RelatedEntities = new List<AuditEntity> { new() { Id = existingInvitation.AccountId.ToString(), Type = "Account" } },
                AffectedEntity = new AuditEntity { Type = "Invitation", Id = existingInvitation.Id.ToString() }
            }
        }, cancellationToken);
    }
}