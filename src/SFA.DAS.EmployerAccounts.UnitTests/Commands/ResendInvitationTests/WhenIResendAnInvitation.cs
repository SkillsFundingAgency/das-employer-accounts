﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.AuditCommand;
using SFA.DAS.EmployerAccounts.Commands.ResendInvitation;
using SFA.DAS.EmployerAccounts.Commands.SendNotification;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Models.AccountTeam;
using SFA.DAS.EmployerAccounts.Models.UserProfile;
using SFA.DAS.EmployerAccounts.UnitTests.Fakes;
using SFA.DAS.TimeProvider;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.ResendInvitationTests
{
    //TODO Refactor, lots of CTRL C CTRL V
    [TestFixture]
    public class WhenIResendAnInvitation
    {
        private Mock<IMembershipRepository> _membershipRepository;
        private Mock<IInvitationRepository> _invitationRepository;
        private ResendInvitationCommandHandler _handler;
        private Mock<IMediator> _mediator;
        private EmployerAccountsConfiguration _config;
        private ResendInvitationCommand _command;
        private Mock<IUserAccountRepository> _userRepository;
        private const int ExpectedAccountId = 14546;
        private const string ExpectedHashedId = "145AVF46";
        private const string ExpectedExistingUserEmail = "registered@test.local";

        [SetUp]
        public void Setup()
        {
            _command = new ResendInvitationCommand
            {
                Email = "test.user@test.local",
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
            _mediator = new Mock<IMediator>();

            _config = new EmployerAccountsConfiguration();

            _handler = new ResendInvitationCommandHandler(_invitationRepository.Object, _membershipRepository.Object, _mediator.Object, _config, _userRepository.Object);
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
            var exception = Assert.ThrowsAsync<InvalidRequestException>(() => _handler.Handle(command, CancellationToken.None));

            //Assert
            Assert.That(exception.ErrorMessages.Count, Is.EqualTo(3));
            Assert.That(exception.ErrorMessages.Count(x => x.Key == "Email"), Is.EqualTo(1));
            Assert.That(exception.ErrorMessages.Count(x => x.Key == "HashedId"), Is.EqualTo(1));
            Assert.That(exception.ErrorMessages.Count(x => x.Key == "ExternalUserId"), Is.EqualTo(1));
        }

        [Test]
        public void CallerIsNotAnAccountOwner()
        {
            //Act
            var exception = Assert.ThrowsAsync<InvalidRequestException>(async () => await _handler.Handle(_command, CancellationToken.None));

            //Assert
            Assert.That(exception.ErrorMessages.Count, Is.EqualTo(1));
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
            Assert.That(exception.ErrorMessages.Count(x => x.Key == "Invitation"), Is.EqualTo(1));
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
            Assert.That(exception.ErrorMessages.Count(x => x.Key == "Invitation"), Is.EqualTo(1));
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
                AccountId = 1,
                Status = InvitationStatus.Deleted,
                ExpiryDate = DateTimeProvider.Current.UtcNow.AddDays(-1)
            };
            _invitationRepository.Setup(x => x.Get(ExpectedAccountId, _command.Email)).ReturnsAsync(invitation);

            //Act
            await _handler.Handle(_command, CancellationToken.None);

            //Assert
            _invitationRepository.Verify(x => x.Resend(It.Is<Invitation>(c => c.Id == invitationId && c.Status == InvitationStatus.Pending && c.ExpiryDate == DateTimeProvider.Current.UtcNow.Date.AddDays(8))), Times.Once);
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
            _mediator.Verify(x => x.Send(It.Is<SendNotificationCommand>(c =>
                c.RecipientsAddress.Equals(_command.Email) && c.TemplateId.Equals("InvitationNewUser")), It.IsAny<CancellationToken>()));
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
            _mediator.Verify(x => x.Send(It.Is<SendNotificationCommand>(c =>
                c.RecipientsAddress.Equals(ExpectedExistingUserEmail)
                && c.TemplateId.Equals("InvitationExistingUser")
            ), It.IsAny<CancellationToken>()));
        }
    }
}