using System.Threading;
using SFA.DAS.EmployerAccounts.Audit.Types;
using SFA.DAS.EmployerAccounts.Commands.AuditCommand;
using SFA.DAS.EmployerAccounts.Commands.SendNotification;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.Encoding;
using SFA.DAS.Notifications.Api.Types;
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

    public SupportResendInvitationCommandHandler(IInvitationRepository invitationRepository, 
        IMediator mediator, 
        EmployerAccountsConfiguration employerApprenticeshipsServiceConfiguration, 
        IUserAccountRepository userRepository,
        IEmployerAccountRepository accountRepository,
        IEncodingService encodingService)
    {
        _invitationRepository = invitationRepository ?? throw new ArgumentNullException(nameof(invitationRepository));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _employerApprenticeshipsServiceConfiguration = employerApprenticeshipsServiceConfiguration ?? throw new ArgumentNullException(nameof(employerApprenticeshipsServiceConfiguration));
        _userRepository = userRepository;
        _accountRepository = accountRepository;
        _encodingService = encodingService;
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
        
        await AddAuditEntry(message, cancellationToken, existingInvitation);

        var existingUser = await _userRepository.Get(message.Email);
        
        await SendNotification(message, cancellationToken, existingUser, account, expiryDate);
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

    private async Task SendNotification(SupportResendInvitationCommand message, CancellationToken cancellationToken, User existingUser, Account account, DateTime expiryDate)
    {
        await _mediator.Send(new SendNotificationCommand
        {
            Email = new Email
            {
                RecipientsAddress = message.Email,
                TemplateId = existingUser?.UserRef != null ? "InvitationExistingUser" : "InvitationNewUser",
                ReplyToAddress = "noreply@sfa.gov.uk",
                Subject = "x",
                SystemId = "x",
                Tokens = new Dictionary<string, string> {
                    { "account_name", account.Name },
                    { "first_name", message.FirstName },
                    { "inviter_name", "Apprenticeship Service Support"},
                    { "base_url", _employerApprenticeshipsServiceConfiguration.DashboardUrl },
                    { "expiry_date", expiryDate.ToString("dd MMM yyy")}
                }
            }
        }, cancellationToken);
    }

    private async Task AddAuditEntry(SupportResendInvitationCommand message, CancellationToken cancellationToken, Invitation existingInvitation)
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
                    new() {PropertyName = "Status",NewValue = existingInvitation.Status.ToString()},
                    new() {PropertyName = "ExpiryDate",NewValue = existingInvitation.ExpiryDate.ToString()}
                },
                RelatedEntities = new List<AuditEntity> { new() { Id = existingInvitation.AccountId.ToString(), Type = "Account" } },
                AffectedEntity = new AuditEntity { Type = "Invitation", Id = existingInvitation.Id.ToString() }
            }
        }, cancellationToken);
    }
}