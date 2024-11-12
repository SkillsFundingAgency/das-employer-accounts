﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Time.Testing;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.AuditCommand;
using SFA.DAS.EmployerAccounts.Commands.CreateInvitation;
using SFA.DAS.EmployerAccounts.Commands.SendNotification;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Models.AccountTeam;
using SFA.DAS.EmployerAccounts.Models.UserProfile;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.CreateInvitationTests;

[TestFixture]
public class WhenICallCreateInvitation
{
    private Mock<IInvitationRepository> _invitationRepository;
    private CreateInvitationCommandHandler _handler;
    private CreateInvitationCommand _command;
    private Mock<IMembershipRepository> _membershipRepository;
    private Mock<IMediator> _mediator;
    private EmployerAccountsConfiguration _configuration;
    private Mock<IValidator<CreateInvitationCommand>> _validator;
    private Mock<IUserAccountRepository> _userRepository;
    private Mock<IEventPublisher> _endpoint;
    private const long ExpectedAccountId = 545641561;
    private const long ExpectedUserId = 521465;
    private const long ExpectedInvitationId = 1231234;
    private const string ExpectedHashedId = "aaa415ss1";
    private const string ExpectedCallerEmail = "test.user@test.local";
    private const string ExpectedExistingUserEmail = "registered@test.local";
    private const string NameOfPersonBeingInvited = "Invited User";
    private FakeTimeProvider _timeProvider;


    private static readonly Guid ExpectedExternalUserId = Guid.NewGuid();
    private Mock<IConfiguration> _config;

    [SetUp]
    public void Setup()
    {
        _timeProvider = new FakeTimeProvider(DateTime.UtcNow);
        _invitationRepository = new Mock<IInvitationRepository>();
        _invitationRepository.Setup(x => x.Get(ExpectedAccountId, ExpectedCallerEmail)).ReturnsAsync(() => null);
        _invitationRepository.Setup(x => x.Create(It.IsAny<Invitation>())).ReturnsAsync(ExpectedInvitationId);

        _membershipRepository = new Mock<IMembershipRepository>();
        _membershipRepository.Setup(x => x.GetCaller(ExpectedHashedId, ExpectedExternalUserId.ToString()))
            .ReturnsAsync(new MembershipView
            {
                AccountId = ExpectedAccountId,
                UserId = ExpectedUserId,
                UserRef = ExpectedExternalUserId
            });

        _userRepository = new Mock<IUserAccountRepository>();
        _userRepository.Setup(x => x.Get(ExpectedExistingUserEmail)).ReturnsAsync(new User { Email = ExpectedExistingUserEmail, UserRef = Guid.NewGuid().ToString() });

        _endpoint = new Mock<IEventPublisher>();

        _mediator = new Mock<IMediator>();

        _validator = new Mock<IValidator<CreateInvitationCommand>>();
        _validator.Setup(x => x.ValidateAsync(It.IsAny<CreateInvitationCommand>())).ReturnsAsync(new ValidationResult());

        _configuration = new EmployerAccountsConfiguration();
        _config = new Mock<IConfiguration>();
        _config.Setup(x => x["ResourceEnvironmentName"]).Returns("test");

        _handler = new CreateInvitationCommandHandler(_invitationRepository.Object, _membershipRepository.Object, _mediator.Object, _configuration, _validator.Object, _userRepository.Object, _endpoint.Object, _config.Object, _timeProvider);
        _command = new CreateInvitationCommand
        {
            HashedAccountId = ExpectedHashedId,
            EmailOfPersonBeingInvited = ExpectedCallerEmail,
            NameOfPersonBeingInvited = NameOfPersonBeingInvited,
            RoleOfPersonBeingInvited = Role.Owner,
            ExternalUserId = ExpectedExternalUserId.ToString()
        };
    }

    [Test]
    public async Task Then_InvitedUserEventPublishedWithCorrectPersonInvited()
    {
        //Act
        await _handler.Handle(_command, CancellationToken.None);

        _endpoint.Verify(e => e.Publish(It.Is<InvitedUserEvent>(i => i.PersonInvited == NameOfPersonBeingInvited)));
    }

    [Test]
    public async Task ValidCommandFromAccountOwnerCreatesInvitation()
    {
        //Act
        await _handler.Handle(_command, CancellationToken.None);

        //Assert
        _invitationRepository.Verify(x => x.Create(It.Is<Invitation>(m => m.AccountId == ExpectedAccountId && m.Email == _command.EmailOfPersonBeingInvited && m.Name == _command.NameOfPersonBeingInvited && m.Status == InvitationStatus.Pending && m.Role == _command.RoleOfPersonBeingInvited && m.ExpiryDate == _timeProvider.GetUtcNow().Date.AddDays(8))), Times.Once);
    }

    [Test]
    public void ValidCommandButExistingDoesNotCreateInvitation()
    {
        _invitationRepository.Setup(x => x.Get(ExpectedAccountId, _command.EmailOfPersonBeingInvited)).ReturnsAsync(new Invitation
        {
            Id = 1,
            AccountId = ExpectedAccountId,
            Email = _command.EmailOfPersonBeingInvited
        });

        var action = () => _handler.Handle(_command, CancellationToken.None);
        action.Should().ThrowAsync<InvalidRequestException>().WithMessage("");
    }

    [Test]
    public void ThenAUnauthorizedAccessExecptionIsThrownIfTheValidionResultIsUnauthorized()
    {
        //Arrange
        _validator.Setup(x => x.ValidateAsync(It.IsAny<CreateInvitationCommand>())).ReturnsAsync(new ValidationResult { IsUnauthorized = true });

        //Act
        var action = () => _handler.Handle(_command, CancellationToken.None);
        action.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Test]
    public async Task ThenTheSendNotificationCommandIsInvoked()
    {
        const int userId = 1;

        _membershipRepository.Setup(x => x.GetCaller(_command.HashedAccountId, _command.ExternalUserId)).ReturnsAsync(new MembershipView
        {
            Role = Role.Owner,
            UserId = userId,
            UserRef = ExpectedExternalUserId,
            AccountId = ExpectedAccountId
        });

        //Act
        await _handler.Handle(_command, CancellationToken.None);

        //Assert
        _mediator.Verify(x => x.Send(It.Is<SendNotificationCommand>(c => c.RecipientsAddress.Equals(ExpectedCallerEmail)
                                                                         && c.TemplateId.Equals("2bb7da99-2542-4536-9c15-4eb3466a99e3")
        ), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task ThenTheSendNotificationCommandIsInvoked_ForGovLogin_NewUserNonProd()
    {
        const int userId = 1;

        _membershipRepository.Setup(x => x.GetCaller(_command.HashedAccountId, _command.ExternalUserId)).ReturnsAsync(new MembershipView
        {
            Role = Role.Owner,
            UserId = userId,
            UserRef = ExpectedExternalUserId,
            AccountId = ExpectedAccountId
        });

        //Act
        await _handler.Handle(_command, CancellationToken.None);

        //Assert
        _mediator.Verify(x => x.Send(It.Is<SendNotificationCommand>(c => c.RecipientsAddress.Equals(ExpectedCallerEmail)
                                                                         && c.TemplateId.Equals("2bb7da99-2542-4536-9c15-4eb3466a99e3")
        ), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task ThenTheSendNotificationCommandIsInvoked_ForGovLogin_NewUserProd()
    {
        const int userId = 1;

        _config.Setup(x => x["ResourceEnvironmentName"]).Returns("prd");
        _membershipRepository.Setup(x => x.GetCaller(_command.HashedAccountId, _command.ExternalUserId)).ReturnsAsync(new MembershipView
        {
            Role = Role.Owner,
            UserId = userId,
            UserRef = ExpectedExternalUserId,
            AccountId = ExpectedAccountId
        });

        _handler = new CreateInvitationCommandHandler(_invitationRepository.Object, _membershipRepository.Object, _mediator.Object, _configuration, _validator.Object, _userRepository.Object, _endpoint.Object, _config.Object, Mock.Of<System.TimeProvider>());

        //Act
        await _handler.Handle(_command, CancellationToken.None);

        //Assert
        _mediator.Verify(x => x.Send(It.Is<SendNotificationCommand>(c => c.RecipientsAddress.Equals(ExpectedCallerEmail)
                                                                         && c.TemplateId.Equals("6b6b46cc-4a5f-4985-8626-ed239af11d71")
        ), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task ThenTheSendNotificationCommandIsInvoked_ForGovLogin_ExistingUserNonProd()
    {
        const int userId = 1;

        _membershipRepository.Setup(x => x.GetCaller(_command.HashedAccountId, _command.ExternalUserId)).ReturnsAsync(new MembershipView
        {
            Role = Role.Owner,
            UserId = userId,
            UserRef = ExpectedExternalUserId,
            AccountId = ExpectedAccountId
        });
        _command.EmailOfPersonBeingInvited = ExpectedExistingUserEmail;

        //Act
        await _handler.Handle(_command, CancellationToken.None);

        //Assert
        _mediator.Verify(x => x.Send(It.Is<SendNotificationCommand>(c => c.RecipientsAddress.Equals(ExpectedExistingUserEmail)
                                                                         && c.TemplateId.Equals("11cb4eb4-c22a-47c7-aa26-1074da25ff4d")), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task ThenTheSendNotificationCommandIsInvoked_ForGovLogin_ExistingUserProd()
    {
        const int userId = 1;
        _config.Setup(x => x["ResourceEnvironmentName"]).Returns("prd");
        _membershipRepository.Setup(x => x.GetCaller(_command.HashedAccountId, _command.ExternalUserId)).ReturnsAsync(new MembershipView
        {
            Role = Role.Owner,
            UserId = userId,
            UserRef = ExpectedExternalUserId,
            AccountId = ExpectedAccountId
        });
        _handler = new CreateInvitationCommandHandler(_invitationRepository.Object, _membershipRepository.Object, _mediator.Object, _configuration, _validator.Object, _userRepository.Object, _endpoint.Object, _config.Object, Mock.Of<System.TimeProvider>());
        _command.EmailOfPersonBeingInvited = ExpectedExistingUserEmail;

        //Act
        await _handler.Handle(_command, CancellationToken.None);

        //Assert
        _mediator.Verify(x => x.Send(It.Is<SendNotificationCommand>(c =>
            c.RecipientsAddress.Equals(ExpectedExistingUserEmail)
            && c.TemplateId.Equals("3c285db3-164c-4258-9180-f2d42723e155")
        ), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task ThenADifferentEmailIsSentIfTheEmailIsAlreadyRegisteredInTheSystem()
    {
        //Arrange
        _command.EmailOfPersonBeingInvited = ExpectedExistingUserEmail;

        //Act
        await _handler.Handle(_command, CancellationToken.None);

        //Assert
        _mediator.Verify(x => x.Send(It.Is<SendNotificationCommand>(c =>
            c.RecipientsAddress.Equals(ExpectedExistingUserEmail)
            && c.TemplateId.Equals("11cb4eb4-c22a-47c7-aa26-1074da25ff4d")
        ), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task ThenTheAuditCommandIsCalledWhenTheCreateInvitationCommandIsValid()
    {
        //Arrange
        const int userId = 1;

        _membershipRepository.Setup(x => x.GetCaller(_command.HashedAccountId, _command.ExternalUserId))
            .ReturnsAsync(new MembershipView
            {
                Role = Role.Owner,
                UserId = userId,
                UserRef = ExpectedExternalUserId,
                AccountId = ExpectedAccountId
            });

        //Act
        await _handler.Handle(_command, CancellationToken.None);

        //Assert
        _mediator.Verify(x => x.Send(It.Is<CreateAuditCommand>(c =>
            c.EasAuditMessage.ChangedProperties.SingleOrDefault(y => y.PropertyName.Equals("AccountId") && y.NewValue.Equals(ExpectedAccountId.ToString())) != null &&
            c.EasAuditMessage.ChangedProperties.SingleOrDefault(y => y.PropertyName.Equals("Email") && y.NewValue.Equals(ExpectedCallerEmail)) != null &&
            c.EasAuditMessage.ChangedProperties.SingleOrDefault(y => y.PropertyName.Equals("Name") && y.NewValue.Equals(_command.NameOfPersonBeingInvited.ToString())) != null &&
            c.EasAuditMessage.ChangedProperties.SingleOrDefault(y => y.PropertyName.Equals("Role") && y.NewValue.Equals(_command.RoleOfPersonBeingInvited.ToString())) != null &&
            c.EasAuditMessage.ChangedProperties.SingleOrDefault(y => y.PropertyName.Equals("Status") && y.NewValue.Equals(InvitationStatus.Pending.ToString())) != null &&
            c.EasAuditMessage.ChangedProperties.SingleOrDefault(y => y.PropertyName.Equals("ExpiryDate") && y.NewValue.Equals(System.TimeProvider.System.GetUtcNow().Date.AddDays(8).ToString("yyyy-MM-dd HH:mm:ss.fffff"))) != null
        ), It.IsAny<CancellationToken>()));

        _mediator.Verify(x => x.Send(It.Is<CreateAuditCommand>(c =>
            c.EasAuditMessage.Description.Equals($"Member {ExpectedCallerEmail} added to account {ExpectedAccountId} as {_command.RoleOfPersonBeingInvited.ToString()}")), It.IsAny<CancellationToken>()));

        _mediator.Verify(x => x.Send(It.Is<CreateAuditCommand>(c =>
            c.EasAuditMessage.RelatedEntities.SingleOrDefault(y => y.Id.Equals(ExpectedAccountId.ToString()) && y.Type.Equals("Account")) != null
        ), It.IsAny<CancellationToken>()));

        _mediator.Verify(x => x.Send(It.Is<CreateAuditCommand>(c =>
            c.EasAuditMessage.AffectedEntity.Id.Equals(ExpectedInvitationId.ToString()) &&
            c.EasAuditMessage.AffectedEntity.Type.Equals("Invitation")
        ), It.IsAny<CancellationToken>()));
    }
}