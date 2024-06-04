using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Audit.Types;
using SFA.DAS.EmployerAccounts.Commands.SupportCreateInvitation;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Models.AccountTeam;
using SFA.DAS.EmployerAccounts.Models.UserProfile;
using SFA.DAS.EmployerAccounts.UnitTests.Fakes;
using SFA.DAS.Encoding;
using SFA.DAS.Notifications.Messages.Commands;
using SFA.DAS.NServiceBus.Services;
using SFA.DAS.TimeProvider;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.SupportCreateInvitationTests;

public class WhenICallSupportSendInvitation
{
    private SupportCreateInvitationCommandHandler _handler;
    private Mock<IInvitationRepository> _invitationRepository;
    private EmployerAccountsConfiguration _employerAccountsConfig;
    private Mock<IUserAccountRepository> _userAccountRepository;
    private Mock<IEmployerAccountRepository> _employerAccountRepository;
    private Mock<IEncodingService> _encodingService;
    private Mock<IMessageSession> _publisher;
    private Mock<IEventPublisher> _eventPublisher;
    private Mock<IAuditService> _auditService;
    private Mock<IValidator<SupportCreateInvitationCommand>> _validator;
    private SupportCreateInvitationCommand _command;
    private Mock<IConfiguration> _config;

    private const int AccountId = 14546;
    private const string HashedId = "111DEDW";
    private const string SupportUserEmail = "support@local.test";
    private const string Email = "user@local.test";
    private const string UserFullName = "Test User";
    private const string AccountName = "Test Account";
    private readonly Guid SupportUserRef = Guid.NewGuid();

    [SetUp]
    public void Setup()
    {
        _command = new SupportCreateInvitationCommand
        {
            EmailOfPersonBeingInvited = Email,
            HashedAccountId = HashedId,
            SupportUserEmail = SupportUserEmail,
            NameOfPersonBeingInvited = UserFullName,
            RoleOfPersonBeingInvited = Role.Owner,
        };

        _invitationRepository = new Mock<IInvitationRepository>();
        _employerAccountsConfig = new EmployerAccountsConfiguration();
        _userAccountRepository = new Mock<IUserAccountRepository>();
        _employerAccountRepository = new Mock<IEmployerAccountRepository>();
        _encodingService = new Mock<IEncodingService>();
        _publisher = new Mock<IMessageSession>();
        _auditService = new Mock<IAuditService>();
        _validator = new Mock<IValidator<SupportCreateInvitationCommand>>();
        _eventPublisher = new Mock<IEventPublisher>();
        _config = new Mock<IConfiguration>();
        
        _config.Setup(x => x["ResourceEnvironmentName"]).Returns("test");
        _userAccountRepository.Setup(x => x.Get(SupportUserEmail)).ReturnsAsync(new User { Email = Email, Ref = SupportUserRef });
        _encodingService.Setup(x => x.Decode(HashedId, EncodingType.AccountId)).Returns(AccountId);
        _employerAccountRepository.Setup(x => x.GetAccountById(AccountId)).ReturnsAsync(new Account { Id = AccountId, Name = AccountName });
        _validator.Setup(x => x.ValidateAsync(It.IsAny<SupportCreateInvitationCommand>())).ReturnsAsync(new ValidationResult());
        
        DateTimeProvider.Current = new FakeTimeProvider(DateTime.UtcNow);
        
        _handler = new SupportCreateInvitationCommandHandler(
            _validator.Object,
            _invitationRepository.Object,
            _auditService.Object,
            _encodingService.Object,
            _employerAccountsConfig, 
            _eventPublisher.Object,
            _userAccountRepository.Object,
            _employerAccountRepository.Object,
            _publisher.Object
            );
    }
    
    [TearDown]
    public void Teardown()
    {
        DateTimeProvider.ResetToDefault();
    }
    
    [Test]
    public async Task Then_InvitedUserEventPublishedWithCorrectPersonInvited()
    {
        await _handler.Handle(_command, CancellationToken.None);

        _eventPublisher.Verify(e =>e.Publish(It.Is<InvitedUserEvent>(i => i.PersonInvited == UserFullName)));
    }
    
    [Test]
    public async Task ValidCommandFromAccountOwnerCreatesInvitation()
    {
        await _handler.Handle(_command, CancellationToken.None);

        _invitationRepository.Verify(x => x.Create(It.Is<Invitation>(m => m.AccountId == AccountId && m.Email == _command.EmailOfPersonBeingInvited && m.Name == _command.NameOfPersonBeingInvited && m.Status == InvitationStatus.Pending && m.Role == _command.RoleOfPersonBeingInvited && m.ExpiryDate == DateTimeProvider.Current.UtcNow.Date.AddDays(8))), Times.Once);
    }
    
    [Test]
    public void ValidCommandButExistingDoesNotCreateInvitation()
    {
        _invitationRepository.Setup(x => x.Get(AccountId, _command.EmailOfPersonBeingInvited)).ReturnsAsync(new Invitation
        {
            Id = 1,
            AccountId = AccountId,
            Email = _command.EmailOfPersonBeingInvited
        });

        var exception = Assert.ThrowsAsync<InvalidRequestException>(async () => await _handler.Handle(_command, CancellationToken.None));

        Assert.That(exception.ErrorMessages.Count, Is.EqualTo(1));
    }
    
    [Test]
    public async Task ThenTheSendNotificationCommandIsCalled()
    {
        await _handler.Handle(_command, CancellationToken.None);

        _publisher.Verify(x => x.Send(It.Is<SendEmailCommand>(c => c.RecipientsAddress.Equals(_command.EmailOfPersonBeingInvited)
                                                                   && c.TemplateId.Equals("InvitationNewUser")), It.IsAny<SendOptions>()));
    }
    
    [Test]
    public async Task ThenTheAuditCommandIsCalledWhenTheResendCommandIsValid()
    {
        await _handler.Handle(_command, CancellationToken.None);

        _auditService.Verify(x => x.SendAuditMessage(It.Is<AuditMessage>(c =>
            c.SupportUserEmail == SupportUserEmail 
            && c.ChangedProperties.SingleOrDefault(y => y.PropertyName.Equals("AccountId") && y.NewValue.Equals(AccountId.ToString())) != null
            && c.ChangedProperties.SingleOrDefault(y => y.PropertyName.Equals("Status") && y.NewValue.Equals(InvitationStatus.Pending.ToString())) != null
            && c.ChangedProperties.SingleOrDefault(y => y.PropertyName.Equals("Email") && y.NewValue.Equals(_command.EmailOfPersonBeingInvited)) != null
            && c.ChangedProperties.SingleOrDefault(y => y.PropertyName.Equals("Name") && y.NewValue.Equals(_command.NameOfPersonBeingInvited)) != null
            && c.ChangedProperties.SingleOrDefault(y => y.PropertyName.Equals("Role") && y.NewValue.Equals(_command.RoleOfPersonBeingInvited.ToString())) != null
        )));
    }
}