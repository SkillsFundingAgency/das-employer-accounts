using System.Threading;
using Microsoft.Extensions.Configuration;
using NServiceBus;
using SFA.DAS.EmployerAccounts.Audit.Types;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.Encoding;
using SFA.DAS.Notifications.Messages.Commands;
using SFA.DAS.NServiceBus.Services;
using SFA.DAS.TimeProvider;

namespace SFA.DAS.EmployerAccounts.Commands.SupportCreateInvitation;

public class SupportCreateInvitationCommandHandler : IRequestHandler<SupportCreateInvitationCommand>
{
    private readonly IValidator<SupportCreateInvitationCommand> _validator;
    private readonly IInvitationRepository _invitationRepository;
    private readonly IAuditService _auditService;
    private readonly IEncodingService _encodingService;
    private readonly EmployerAccountsConfiguration _employerAccountsConfiguration;
    private readonly IEventPublisher _eventPublisher;
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IEmployerAccountRepository _accountRepository;
    private readonly IMessageSession _publisher;
    private readonly bool _isProdEnvironment;

    public SupportCreateInvitationCommandHandler(IValidator<SupportCreateInvitationCommand> validator,
        IInvitationRepository invitationRepository,
        IAuditService auditService,
        IEncodingService encodingService,
        EmployerAccountsConfiguration employerAccountsConfiguration,
        IEventPublisher eventPublisher,
        IUserAccountRepository userAccountRepository,
        IConfiguration configuration,
        IEmployerAccountRepository accountRepository,
        IMessageSession publisher)
    {
        _validator = validator;
        _invitationRepository = invitationRepository;
        _auditService = auditService;
        _encodingService = encodingService;
        _employerAccountsConfiguration = employerAccountsConfiguration;
        _eventPublisher = eventPublisher;
        _userAccountRepository = userAccountRepository;
        _accountRepository = accountRepository;
        _publisher = publisher;

        _isProdEnvironment = !string.IsNullOrEmpty(configuration["ResourceEnvironmentName"])
                             && configuration["ResourceEnvironmentName"].Equals("prd", StringComparison.CurrentCultureIgnoreCase);
    }

    public async Task Handle(SupportCreateInvitationCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request);

        if (!validationResult.IsValid())
        {
            throw new InvalidRequestException(validationResult.ValidationDictionary);
        }

        if (validationResult.IsUnauthorized)
        {
            throw new UnauthorizedAccessException();
        }

        var accountId = _encodingService.Decode(request.HashedAccountId, EncodingType.AccountId);

        //Verify the email is not used by an existing invitation for the account
        var existingInvitation = await _invitationRepository.Get(accountId, request.EmailOfPersonBeingInvited);

        if (existingInvitation != null && existingInvitation.Status != InvitationStatus.Deleted && existingInvitation.Status != InvitationStatus.Accepted)
            throw new InvalidRequestException(new Dictionary<string, string> { { "ExistingMember", $"{request.EmailOfPersonBeingInvited} is already invited" } });

        var expiryDate = DateTimeProvider.Current.UtcNow.Date.AddDays(8);

        var invitationId = 0L;

        if (existingInvitation == null)
        {
            invitationId = await _invitationRepository.Create(new Invitation
            {
                AccountId = accountId,
                Email = request.EmailOfPersonBeingInvited,
                Name = request.NameOfPersonBeingInvited,
                Role = request.RoleOfPersonBeingInvited,
                Status = InvitationStatus.Pending,
                ExpiryDate = expiryDate
            });
        }
        else
        {
            existingInvitation.Name = request.NameOfPersonBeingInvited;
            existingInvitation.Role = request.RoleOfPersonBeingInvited;
            existingInvitation.Status = InvitationStatus.Pending;
            existingInvitation.ExpiryDate = expiryDate;

            await _invitationRepository.Resend(existingInvitation);

            invitationId = existingInvitation.Id;
        }

        await AddAuditEntry(request, accountId, expiryDate, invitationId);

        await SendInvitation(request, expiryDate, accountId);

        var supportUser = await _userAccountRepository.Get(request.SupportUserEmail);

        await PublishUserInvitedEvent(accountId, request.NameOfPersonBeingInvited, request.SupportUserEmail, supportUser.Ref);
    }

    private async Task SendInvitation(SupportCreateInvitationCommand message, DateTime expiryDate, long accountId)
    {
        var govLoginExistingUser = "11cb4eb4-c22a-47c7-aa26-1074da25ff4d"; //test ---- 3c285db3-164c-4258-9180-f2d42723e155 prod
        var existingUser = await _userAccountRepository.Get(message.EmailOfPersonBeingInvited);

        var templateId = existingUser?.UserRef != null
            ? "InvitationExistingUser"
            : "InvitationNewUser";

        if (_employerAccountsConfiguration.UseGovSignIn)
        {
            if (_isProdEnvironment)
            {
                templateId = existingUser?.UserRef != null
                    ? "3c285db3-164c-4258-9180-f2d42723e155"
                    : "6b6b46cc-4a5f-4985-8626-ed239af11d71";
            }
            else
            {
                templateId = existingUser?.UserRef != null
                    ? "11cb4eb4-c22a-47c7-aa26-1074da25ff4d"
                    : "2bb7da99-2542-4536-9c15-4eb3466a99e3";
            }
        }

        var account = await _accountRepository.GetAccountById(accountId);

        var tokens = new Dictionary<string, string>
        {
            { "account_name", account.Name },
            { "first_name", existingUser != null ? existingUser.FirstName : message.EmailOfPersonBeingInvited },
            { "inviter_name", "Apprenticeship Service Support" },
            { "base_url", _employerAccountsConfiguration.DashboardUrl },
            { "expiry_date", expiryDate.ToString("dd MMM yyy") }
        };

        await _publisher.Send(new SendEmailCommand(templateId, message.EmailOfPersonBeingInvited, tokens));
    }

    private async Task AddAuditEntry(SupportCreateInvitationCommand message, long accountId, DateTime expiryDate, long invitationId)
    {
        await _auditService.SendAuditMessage(new AuditMessage
        {
            SupportUserEmail = message.SupportUserEmail,
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
        return _eventPublisher.Publish(new InvitedUserEvent
        {
            AccountId = accountId,
            PersonInvited = personInvited,
            UserName = invitedByUserName,
            UserRef = invitedByUserRef,
            Created = DateTime.UtcNow
        });
    }
}