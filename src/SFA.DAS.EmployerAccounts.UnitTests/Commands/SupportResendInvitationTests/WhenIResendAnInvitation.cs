﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elasticsearch.Net;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Audit.Types;
using SFA.DAS.EmployerAccounts.Commands.SupportResendInvitationCommand;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Models.AccountTeam;
using SFA.DAS.EmployerAccounts.Models.UserProfile;
using SFA.DAS.Encoding;
using SFA.DAS.Notifications.Messages.Commands;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.SupportResendInvitationTests;

[TestFixture]
public class WhenIResendAnInvitation
{
    private Mock<IInvitationRepository> _invitationRepository;
    private Mock<IMessageSession> _publisher;
    private SupportResendInvitationCommandHandler _handler;
    private Mock<IAuditService> _auditService;
    private EmployerAccountsConfiguration _config;
    private SupportResendInvitationCommand _command;
    private Mock<IUserAccountRepository> _userRepository;
    private Mock<IEmployerAccountRepository> _employerAccountRepository;
    private Mock<IEncodingService> _encodingService;
    private FakeTimeProvider _fakeTimeProvider;

    private const int AccountId = 14546;
    private const string HashedId = "145AVF46";
    private const string ExistingUserEmail = "testing.user@test.local";
    private const string AccountOwnerEmail = "owner@test.local";
    private const string AccountName = "Test Account";

    [SetUp]
    public void Setup()
    {
        _fakeTimeProvider = new FakeTimeProvider();
        
        _command = new SupportResendInvitationCommand
        {
            Email = "test.user@test.local",
            HashedAccountId = HashedId,
        };

        _userRepository = new Mock<IUserAccountRepository>();
        _employerAccountRepository = new Mock<IEmployerAccountRepository>();
        _encodingService = new Mock<IEncodingService>();
        _publisher = new Mock<IMessageSession>();

        _userRepository.Setup(x => x.Get(ExistingUserEmail)).ReturnsAsync(new User { Email = ExistingUserEmail, Ref = Guid.NewGuid() });
        _encodingService.Setup(x => x.Decode(HashedId, EncodingType.AccountId)).Returns(AccountId);

        var memberships = new List<Membership> { new() { AccountId = AccountId, User = new User { Email = AccountOwnerEmail }, Role = Role.Owner } };
        _employerAccountRepository.Setup(x => x.GetAccountById(AccountId)).ReturnsAsync(new Account { Id = AccountId, Name = AccountName, Memberships = memberships });

        _invitationRepository = new Mock<IInvitationRepository>();
        _auditService = new Mock<IAuditService>();

        _config = new EmployerAccountsConfiguration();

        _handler = new SupportResendInvitationCommandHandler(
            _invitationRepository.Object,
            _config,
            _userRepository.Object,
            _employerAccountRepository.Object,
            _encodingService.Object,
            _publisher.Object,
            _auditService.Object,
            _fakeTimeProvider
        );
    }

    [Test]
    public void InvalidCommandThrowsException()
    {
        //Arrange
        var command = new SupportResendInvitationCommand();

        //Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        //Assert
        action.Should()
            .ThrowAsync<InvalidRequestException>()
            .Where(x =>
                x.ErrorMessages.Count == 2
                && x.ErrorMessages.ContainsKey("Id")
                && x.ErrorMessages.ContainsKey("AccountId"));
    }

    [Test]
    public void CallerIsNotAnAccountOwner()
    {
        //Act
        var action = () => _handler.Handle(_command, CancellationToken.None);

        //Assert
        action.Should()
            .ThrowAsync<InvalidRequestException>()
            .Where(x =>
                x.ErrorMessages.Count == 1
                && x.ErrorMessages.ContainsKey("Membership"));
    }

    [Test]
    public void InvitationDoesNotExist()
    {
        //Arrange
        _invitationRepository.Setup(x => x.Get(AccountId, _command.Email)).ReturnsAsync(() => null);

        //Act
        var action = () => _handler.Handle(_command, CancellationToken.None);

        //Assert
        action.Should()
            .ThrowAsync<InvalidRequestException>()
            .Where(x =>
                x.ErrorMessages.Count == 1
                && x.ErrorMessages.ContainsKey("Invitation"));
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
        _invitationRepository.Setup(x => x.Get(AccountId, _command.Email)).ReturnsAsync(invitation);

        //Act
        var action = () => _handler.Handle(_command, CancellationToken.None);

        //Assert
        action.Should()
            .ThrowAsync<InvalidRequestException>()
            .Where(x =>
                x.ErrorMessages.Count == 1
                && x.ErrorMessages.ContainsKey("Invitation"));
    }

    [Test]
    public async Task ShouldResendInvitation()
    {
        //Arrange
        const long invitationId = 12;
        var fakeTimeProvider = new FakeTimeProvider();
        var invitation = new Invitation
        {
            Id = invitationId,
            AccountId = AccountId,
            Status = InvitationStatus.Deleted,
            ExpiryDate = fakeTimeProvider.GetUtcNow().AddDays(-1).Date,
            Email = ExistingUserEmail
        };

        _invitationRepository.Setup(x => x.Get(AccountId, _command.Email)).ReturnsAsync(invitation);

        //Act
        await _handler.Handle(_command, CancellationToken.None);

        //Assert
        _invitationRepository.Verify(x => x.Resend(It.Is<Invitation>(c => c.Id == invitationId && c.Status == InvitationStatus.Pending && c.ExpiryDate == fakeTimeProvider.GetUtcNow().Date.AddDays(8))), Times.Once);
        _encodingService.Verify(x => x.Decode(HashedId, EncodingType.AccountId), Times.Once);
        _employerAccountRepository.Verify(x => x.GetAccountById(AccountId), Times.Once);
    }

    [Test]
    public async Task ThenTheSendNotificationCommandIsCalled()
    {
        //Arrange
        var fakeTimeProvider = new FakeTimeProvider();

        var invitation = new Invitation
        {
            Id = 1,
            Email = "test@email",
            AccountId = 1,
            ExpiryDate = fakeTimeProvider.GetUtcNow().AddDays(-1).Date
        };
        _invitationRepository.Setup(x => x.Get(AccountId, _command.Email)).ReturnsAsync(invitation);

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
        var fakeTimeProvider = new FakeTimeProvider();

        var invitation = new Invitation
        {
            Id = 1,
            Email = "test@email",
            AccountId = 1,
            ExpiryDate = fakeTimeProvider.GetUtcNow().AddDays(-1).Date
        };
        _invitationRepository.Setup(x => x.Get(AccountId, _command.Email)).ReturnsAsync(invitation);

        //Act
        await _handler.Handle(_command, CancellationToken.None);

        _auditService.Verify(x => x.SendAuditMessage(It.Is<AuditMessage>(c =>
            c.ImpersonatedUserEmail == AccountOwnerEmail &&
            c.ChangedProperties.SingleOrDefault(y => y.PropertyName.Equals("Status") && y.NewValue.Equals(InvitationStatus.Pending.ToString())) != null &&
            c.ChangedProperties.SingleOrDefault(y => y.PropertyName.Equals("ExpiryDate") && y.NewValue.Equals(fakeTimeProvider.GetUtcNow().Date.AddDays(8).ToString())) != null
        )));
    }

    [Test]
    public async Task ThenADifferentEmailIsSentIfTheEmailIsAlreadyRegisteredInTheSystem()
    {
        //Arrange
        _command.Email = ExistingUserEmail;
        var fakeTimeProvider = new FakeTimeProvider();
        var invitation = new Invitation
        {
            Id = 1,
            Email = ExistingUserEmail,
            AccountId = 1,
            ExpiryDate = fakeTimeProvider.GetUtcNow().AddDays(-1).Date
        };

        _invitationRepository.Setup(x => x.Get(AccountId, _command.Email)).ReturnsAsync(invitation);

        //Act
        await _handler.Handle(_command, CancellationToken.None);

        //Assert
        _publisher.Verify(x => x.Send(It.Is<SendEmailCommand>(c =>
            c.RecipientsAddress.Equals(ExistingUserEmail)
            && c.TemplateId.Equals("InvitationExistingUser")), It.IsAny<SendOptions>()));
    }
}