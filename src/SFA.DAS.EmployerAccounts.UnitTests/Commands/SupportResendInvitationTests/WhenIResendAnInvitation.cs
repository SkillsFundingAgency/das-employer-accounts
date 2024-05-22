using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.AuditCommand;
using SFA.DAS.EmployerAccounts.Commands.SupportResendInvitationCommand;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Models.AccountTeam;
using SFA.DAS.EmployerAccounts.Models.UserProfile;
using SFA.DAS.EmployerAccounts.UnitTests.Fakes;
using SFA.DAS.Encoding;
using SFA.DAS.Notifications.Messages.Commands;
using SFA.DAS.TimeProvider;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.SupportResendInvitationTests;

[TestFixture]
public class WhenIResendAnInvitation
{
    private Mock<IInvitationRepository> _invitationRepository;
    private Mock<IMessageSession> _publisher;
    private SupportResendInvitationCommandHandler _handler;
    private Mock<IMediator> _mediator;
    private EmployerAccountsConfiguration _config;
    private SupportResendInvitationCommand _command;
    private Mock<IUserAccountRepository> _userRepository;
    private Mock<IEmployerAccountRepository> _employerAccountRepository;
    private Mock<IEncodingService> _encodingService;
    private const int ExpectedAccountId = 14546;
    private const string ExpectedHashedId = "145AVF46";
    private const string ExpectedExistingUserEmail = "testing.user@test.local";
    private const string SupportUserEmail = "support@test.local";
    private const string AccountName = "Test Account";

    [SetUp]
    public void Setup()
    {
        _command = new SupportResendInvitationCommand()
        {
            Email = "test.user@test.local",
            HashedAccountId = ExpectedHashedId,
            SupportUserEmail = SupportUserEmail,
        };

        _userRepository = new Mock<IUserAccountRepository>();
        _employerAccountRepository = new Mock<IEmployerAccountRepository>();
        _encodingService = new Mock<IEncodingService>();
        _publisher = new Mock<IMessageSession>();

        _userRepository.Setup(x => x.Get(ExpectedExistingUserEmail)).ReturnsAsync(new User { Email = ExpectedExistingUserEmail, Ref = Guid.NewGuid() });
        _encodingService.Setup(x => x.Decode(ExpectedHashedId, EncodingType.AccountId)).Returns(ExpectedAccountId);
        _employerAccountRepository.Setup(x => x.GetAccountById(ExpectedAccountId)).ReturnsAsync(new Account { Id = ExpectedAccountId, Name = AccountName });

        _invitationRepository = new Mock<IInvitationRepository>();
        _mediator = new Mock<IMediator>();

        _config = new EmployerAccountsConfiguration();

        _handler = new SupportResendInvitationCommandHandler(
            _invitationRepository.Object,
            _mediator.Object,
            _config,
            _userRepository.Object,
            _employerAccountRepository.Object,
            _encodingService.Object,
            _publisher.Object
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
        var command = new SupportResendInvitationCommand();

        //Act
        var exception = Assert.ThrowsAsync<InvalidRequestException>(() => _handler.Handle(command, CancellationToken.None));

        //Assert
        Assert.That(exception.ErrorMessages.Count, Is.EqualTo(3));
        Assert.That(exception.ErrorMessages.FirstOrDefault(x => x.Key == "Id"), Is.Not.Null);
        Assert.That(exception.ErrorMessages.FirstOrDefault(x => x.Key == "AccountId"), Is.Not.Null);
        Assert.That(exception.ErrorMessages.FirstOrDefault(x => x.Key == "SupportUserEmail"), Is.Not.Null);
    }

    [Test]
    public void CallerIsNotAnAccountOwner()
    {
        //Act
        var exception = Assert.ThrowsAsync<InvalidRequestException>(async () => await _handler.Handle(_command, CancellationToken.None));

        //Assert
        Assert.That(exception.ErrorMessages.Count, Is.EqualTo(1));
        Assert.That(exception.ErrorMessages.FirstOrDefault(x => x.Key == "Membership"), Is.Not.Null);
    }

    [Test]
    public void InvitationDoesNotExist()
    {
        //Arrange
        _invitationRepository.Setup(x => x.Get(ExpectedAccountId, _command.Email)).ReturnsAsync(() => null);

        //Act
        var exception = Assert.ThrowsAsync<InvalidRequestException>(async () => await _handler.Handle(_command, CancellationToken.None));

        //Act
        Assert.That(exception.ErrorMessages.Count, Is.EqualTo(1));
        Assert.That(exception.ErrorMessages.FirstOrDefault(x => x.Key == "Invitation"), Is.Not.Null);
    }

    [Test]
    public void AcceptedInvitationsCannotBeResent()
    {
        //Arrange
        var invitation = new Invitation
        {
            Id = 12,
            AccountId = 1,
            Status = InvitationStatus.Accepted
        };
        _invitationRepository.Setup(x => x.Get(ExpectedAccountId, _command.Email)).ReturnsAsync(invitation);

        //Act
        var exception = Assert.ThrowsAsync<InvalidRequestException>(async () => await _handler.Handle(_command, CancellationToken.None));

        //Assert
        Assert.That(exception.ErrorMessages.Count, Is.EqualTo(1));
        Assert.That(exception.ErrorMessages.FirstOrDefault(x => x.Key == "Invitation"), Is.Not.Null);
    }

    [Test]
    public async Task ShouldResendInvitation()
    {
        //Arrange
        const long invitationId = 12;
        DateTimeProvider.Current = new FakeTimeProvider(DateTime.Now);
        var invitation = new Invitation
        {
            Id = invitationId,
            AccountId = ExpectedAccountId,
            Status = InvitationStatus.Deleted,
            ExpiryDate = DateTimeProvider.Current.UtcNow.AddDays(-1),
            Email = ExpectedExistingUserEmail
        };

        _invitationRepository.Setup(x => x.Get(ExpectedAccountId, _command.Email)).ReturnsAsync(invitation);

        //Act
        await _handler.Handle(_command, CancellationToken.None);

        //Assert
        _invitationRepository.Verify(x => x.Resend(It.Is<Invitation>(c => c.Id == invitationId && c.Status == InvitationStatus.Pending && c.ExpiryDate == DateTimeProvider.Current.UtcNow.Date.AddDays(8))), Times.Once);
        _encodingService.Verify(x => x.Decode(ExpectedHashedId, EncodingType.AccountId), Times.Once);
        _employerAccountRepository.Verify(x => x.GetAccountById(ExpectedAccountId), Times.Once);
    }

    [Test]
    public async Task ThenTheSendNotificationCommandIsCalled()
    {
        //Arrange
        var invitation = new Invitation
        {
            Id = 1,
            Email = "test@email",
            AccountId = 1,
            ExpiryDate = DateTimeProvider.Current.UtcNow.AddDays(-1)
        };
        _invitationRepository.Setup(x => x.Get(ExpectedAccountId, _command.Email)).ReturnsAsync(invitation);

        //Act
        await _handler.Handle(_command, CancellationToken.None);

        //Assert
        _publisher.Verify(x => x.Send(It.Is<SendEmailCommand>(c => c.RecipientsAddress.Equals(_command.Email)
                                                                  && c.TemplateId.Equals("InvitationNewUser")), It.IsAny<SendOptions>()));
    }

    [Test]
    public async Task ThenTheAuditCommandIsCalledWhenTheResendCommandIsValid()
    {
        //Arrange
        var invitation = new Invitation
        {
            Id = 1,
            Email = "test@email",
            AccountId = 1,
            ExpiryDate = DateTimeProvider.Current.UtcNow.AddDays(-1)
        };
        _invitationRepository.Setup(x => x.Get(ExpectedAccountId, _command.Email)).ReturnsAsync(invitation);

        //Act
        await _handler.Handle(_command, CancellationToken.None);

        _mediator.Verify(x => x.Send(It.Is<CreateAuditCommand>(c =>
            c.EasAuditMessage.SupportUserEmail == SupportUserEmail &&
            c.EasAuditMessage.ChangedProperties.SingleOrDefault(y => y.PropertyName.Equals("Status") && y.NewValue.Equals(InvitationStatus.Pending.ToString())) != null &&
            c.EasAuditMessage.ChangedProperties.SingleOrDefault(y => y.PropertyName.Equals("ExpiryDate") && y.NewValue.Equals(DateTimeProvider.Current.UtcNow.Date.AddDays(8).ToString())) != null
        ), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task ThenADifferentEmailIsSentIfTheEmailIsAlreadyRegisteredInTheSystem()
    {
        //Arrange
        _command.Email = ExpectedExistingUserEmail;
        var invitation = new Invitation
        {
            Id = 1,
            Email = ExpectedExistingUserEmail,
            AccountId = 1,
            ExpiryDate = DateTimeProvider.Current.UtcNow.AddDays(-1)
        };

        _invitationRepository.Setup(x => x.Get(ExpectedAccountId, _command.Email)).ReturnsAsync(invitation);

        //Act
        await _handler.Handle(_command, CancellationToken.None);

        //Assert
        _publisher.Verify(x => x.Send(It.Is<SendEmailCommand>(c =>
            c.RecipientsAddress.Equals(ExpectedExistingUserEmail)
            && c.TemplateId.Equals("InvitationExistingUser")), It.IsAny<SendOptions>()));
    }
}