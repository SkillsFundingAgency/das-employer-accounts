using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Audit.Types;
using SFA.DAS.EmployerAccounts.Commands.ResendInvitation;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Models.AccountTeam;
using SFA.DAS.EmployerAccounts.Models.UserProfile;
using SFA.DAS.EmployerAccounts.UnitTests.Fakes;
using SFA.DAS.Encoding;
using SFA.DAS.Notifications.Messages.Commands;
using SFA.DAS.TimeProvider;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.ResendInvitationTests;

[TestFixture]
public class WhenIResendAnInvitation
{
    private Mock<IMessageSession> _publisher;
    private Mock<IAuditService> _auditService;
    private Mock<IMembershipRepository> _membershipRepository;
    private Mock<IInvitationRepository> _invitationRepository;
    private ResendInvitationCommandHandler _handler;
    private EmployerAccountsConfiguration _config;
    private ResendInvitationCommand _command;
    private Mock<IUserAccountRepository> _userRepository;
    private Mock<IEncodingService> _encodingService;
    
    private const int ExpectedAccountId = 14546;
    private const string ExpectedHashedId = "145AVF46";
    private const long InvitationId = 3234;
    private const string HashedInvitationId = "678GAG22";
    private const string ExpectedExistingUserEmail = "registered@test.local";

    [SetUp]
    public void Setup()
    {
        _command = new ResendInvitationCommand
        {
            HashedInvitationId = HashedInvitationId,
            HashedAccountId = ExpectedHashedId,
            ExternalUserId = Guid.NewGuid().ToString(),
        };

        var owner = new MembershipView
        {
            AccountId = ExpectedAccountId,
            UserId = 2,
            Role = Role.Owner,
            HashedAccountId = ExpectedHashedId
        };
        _userRepository = new Mock<IUserAccountRepository>();
        _userRepository.Setup(x => x.Get(ExpectedExistingUserEmail)).ReturnsAsync(new User { Email = ExpectedExistingUserEmail, UserRef = Guid.NewGuid().ToString() });

        _membershipRepository = new Mock<IMembershipRepository>();
        _membershipRepository.Setup(x => x.GetCaller(owner.HashedAccountId, _command.ExternalUserId)).ReturnsAsync(owner);
        _invitationRepository = new Mock<IInvitationRepository>();
        _publisher = new Mock<IMessageSession>();
        _auditService = new Mock<IAuditService>();

        _config = new EmployerAccountsConfiguration();

        _encodingService = new Mock<IEncodingService>();
        _encodingService.Setup(x => x.Decode(HashedInvitationId, EncodingType.AccountId)).Returns(InvitationId);

        _handler = new ResendInvitationCommandHandler(
            _invitationRepository.Object,
            _membershipRepository.Object,
            _config,
            _userRepository.Object,
            _encodingService.Object,
            _publisher.Object,
            _auditService.Object
        );
    }

    [TearDown]
    public void Teardown()
    {
        DateTimeProvider.ResetToDefault();
    }

    [Test]
    public void InvalidCommandThrowsException()
    {
        //Arrange
        var command = new ResendInvitationCommand();

        //Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        //Assert
        action.Should().ThrowAsync<InvalidRequestException>()
            .WithMessage("No HashedInvitationId supplied")
            .WithMessage("No HashedId supplied")
            .WithMessage("No ExternalUserId supplied");
    }

    [Test]
    public void CallerIsNotAnAccountOwner()
    {
        //Act
        var action = () => _handler.Handle(_command, CancellationToken.None);

        //Assert
        action.Should().ThrowAsync<InvalidRequestException>();
    }

    [Test]
    public void InvitationDoesNotExist()
    {
        //Arrange
        _invitationRepository.Setup(x => x.Get(ExpectedAccountId, _command.HashedInvitationId)).ReturnsAsync(() => null);

        //Act
        var action = () => _handler.Handle(_command, CancellationToken.None);

        //Assert
        action.Should().ThrowAsync<InvalidRequestException>()
            .WithMessage("Invitation not found");
    }

    [Test]
    public void AcceptedInvitationsCannotBeResent()
    {
        //Arrange
        var invitation = new Invitation
        {
            Id = InvitationId,
            AccountId = 1,
            Status = InvitationStatus.Accepted
        };
        _invitationRepository.Setup(x => x.Get(ExpectedAccountId, _command.HashedInvitationId)).ReturnsAsync(invitation);

        //Act
        var action = () => _handler.Handle(_command, CancellationToken.None);

        //Assert
        action.Should().ThrowAsync<InvalidRequestException>()
            .WithMessage("Accepted invitations cannot be resent");
    }

    [Test]
    public async Task ShouldResendInvitation()
    {
        //Arrange
        DateTimeProvider.Current = new FakeTimeProvider(DateTime.Now);
        var invitation = new Invitation
        {
            Id = InvitationId,
            AccountId = ExpectedAccountId,
            Status = InvitationStatus.Deleted,
            ExpiryDate = DateTimeProvider.Current.UtcNow.AddDays(-1),
            Email = ExpectedExistingUserEmail,
        };

        _invitationRepository.Setup(x => x.Get(ExpectedAccountId, InvitationId)).ReturnsAsync(invitation);

        //Act
        await _handler.Handle(_command, CancellationToken.None);

        //Assert
        _invitationRepository.Verify(x => x.Resend(It.Is<Invitation>(c => c.Id == InvitationId && c.Status == InvitationStatus.Pending && c.ExpiryDate == DateTimeProvider.Current.UtcNow.Date.AddDays(8))), Times.Once);
    }

    [Test]
    public async Task ThenTheSendNotificationCommandIsCalled()
    {
        //Arrange
        var invitation = new Invitation
        {
            Id = InvitationId,
            Email = ExpectedExistingUserEmail,
            AccountId = 1,
            ExpiryDate = DateTimeProvider.Current.UtcNow.AddDays(-1)
        };

        _invitationRepository.Setup(x => x.Get(ExpectedAccountId, InvitationId)).ReturnsAsync(invitation);
        _userRepository.Setup(x => x.Get(ExpectedExistingUserEmail)).ReturnsAsync(() => null);

        //Act
        await _handler.Handle(_command, CancellationToken.None);

        //Assert
        _publisher.Verify(x => x.Send(It.Is<SendEmailCommand>(c => c.RecipientsAddress.Equals(ExpectedExistingUserEmail)
                                                                   && c.TemplateId.Equals("InvitationNewUser")), It.IsAny<SendOptions>()));
    }

    [Test]
    public async Task ThenTheAuditCommandIsCalledWhenTheResendCommandIsValid()
    {
        //Arrange
        var invitation = new Invitation
        {
            Id = InvitationId,
            Email = ExpectedExistingUserEmail,
            AccountId = 1,
            ExpiryDate = DateTimeProvider.Current.UtcNow.AddDays(-1)
        };

        _invitationRepository.Setup(x => x.Get(ExpectedAccountId, InvitationId)).ReturnsAsync(invitation);

        //Act
        await _handler.Handle(_command, CancellationToken.None);

        _auditService.Verify(x => x.SendAuditMessage(It.Is<AuditMessage>(c =>
            c.ChangedProperties.SingleOrDefault(y => y.PropertyName.Equals("Status") && y.NewValue.Equals(InvitationStatus.Pending.ToString())) != null &&
            c.ChangedProperties.SingleOrDefault(y => y.PropertyName.Equals("ExpiryDate") && y.NewValue.Equals(DateTimeProvider.Current.UtcNow.Date.AddDays(8).ToString())) != null
        )));
    }

    [Test]
    public async Task ThenADifferentEmailIsSentIfTheEmailIsAlreadyRegisteredInTheSystem()
    {
        //Arrange
        _command.HashedInvitationId = HashedInvitationId;
        var invitation = new Invitation
        {
            Id = InvitationId,
            Email = ExpectedExistingUserEmail,
            AccountId = 1,
            ExpiryDate = DateTimeProvider.Current.UtcNow.AddDays(-1),
        };

        _invitationRepository.Setup(x => x.Get(ExpectedAccountId, InvitationId)).ReturnsAsync(invitation);

        //Act
        await _handler.Handle(_command, CancellationToken.None);

        //Assert
        _publisher.Verify(x => x.Send(It.Is<SendEmailCommand>(c => c.RecipientsAddress.Equals(ExpectedExistingUserEmail)
                                                                   && c.TemplateId.Equals("InvitationExistingUser")), It.IsAny<SendOptions>()));
    }
}