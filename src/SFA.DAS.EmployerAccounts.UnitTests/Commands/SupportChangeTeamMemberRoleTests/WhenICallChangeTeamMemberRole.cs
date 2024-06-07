using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Audit.Types;
using SFA.DAS.EmployerAccounts.Commands.SupportChangeTeamMemberRole;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Models.AccountTeam;
using SFA.DAS.EmployerAccounts.Models.UserProfile;
using SFA.DAS.EmployerAccounts.Types.Models;
using SFA.DAS.Encoding;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.SupportChangeTeamMemberRoleTests;

[TestFixture]
public class WhenICallChangeTeamMemberRole
{
    private Mock<IMembershipRepository> _membershipRepository;
    private SupportChangeTeamMemberRoleCommandHandler _handler;
    private SupportChangeTeamMemberRoleCommand _command;
    private MembershipView _callerMembership;
    private TeamMember _userMembership;
    private Mock<IEventPublisher> _eventPublisher;
    private Mock<IEncodingService> _encodingService;
    private Mock<IEmployerAccountRepository> _employerAccountRepository;
    private Mock<IAuditService> _auditService;

    private const int AccountId = 2;
    private const string AccountOwnerEmail = "support@test.local";
    private const string HashedAccountId = "AJDS8HH";

    [SetUp]
    public void Setup()
    {
        _command = new SupportChangeTeamMemberRoleCommand
        {
            HashedAccountId = HashedAccountId,
            Email = "test.user@test.local",
            Role = Role.Owner,
        };

        _callerMembership = new MembershipView
        {
            AccountId = AccountId,
            UserRef = Guid.NewGuid(),
            UserId = 1,
            Role = Role.Owner
        };

        _userMembership = new TeamMember
        {
            AccountId = _callerMembership.AccountId,
            UserRef = Guid.NewGuid(),
            Id = _callerMembership.UserId + 1,
            Role = _command.Role
        };

        _membershipRepository = new Mock<IMembershipRepository>();
        _membershipRepository.Setup(x => x.Get(_callerMembership.AccountId, _command.Email)).ReturnsAsync(_userMembership);

        var memberships = new List<Membership> { new() { AccountId = AccountId, User = new User { Email = AccountOwnerEmail }, Role = Role.Owner } };
        _employerAccountRepository = new Mock<IEmployerAccountRepository>();
        _employerAccountRepository.Setup(x => x.GetAccountById(AccountId)).ReturnsAsync(new Account { Id = AccountId, Memberships = memberships });

        _encodingService = new Mock<IEncodingService>();
        _encodingService.Setup(x => x.Decode(HashedAccountId, EncodingType.AccountId)).Returns(AccountId);

        _eventPublisher = new Mock<IEventPublisher>();
        _auditService = new Mock<IAuditService>();
        _handler = new SupportChangeTeamMemberRoleCommandHandler(
            _membershipRepository.Object,
            _eventPublisher.Object,
            _encodingService.Object,
            _employerAccountRepository.Object,
            _auditService.Object);
    }

    [Test]
    public async Task WillChangeMembershipRole()
    {
        await _handler.Handle(_command, CancellationToken.None);

        _membershipRepository.Verify(x => x.ChangeRole(_userMembership.Id, _callerMembership.AccountId, _command.Role), Times.Once);
        _encodingService.Verify(x => x.Decode(HashedAccountId, EncodingType.AccountId), Times.Once);
    }

    [Test]
    public async Task WillPublishUserRoleUpdatedEvent()
    {
        await _handler.Handle(_command, CancellationToken.None);

        _eventPublisher.Verify(x => x.Publish(It.Is<AccountUserRolesUpdatedEvent>(
                p => p.AccountId == _userMembership.AccountId &&
                     p.UserRef == _userMembership.UserRef &&
                     p.Role == (UserRole)_command.Role))
            , Times.Once);
    }

    [Test]
    public void ThenAnInvalidRequestWillThrowException()
    {
        var command = new SupportChangeTeamMemberRoleCommand();

        var exception = Assert.ThrowsAsync<InvalidRequestException>(() => _handler.Handle(command, CancellationToken.None));

        exception.ErrorMessages.Count.Should().Be(2);

        exception.ErrorMessages.FirstOrDefault(x => x.Key == "AccountId").Should().NotBeNull();
        exception.ErrorMessages.FirstOrDefault(x => x.Key == "Email").Should().NotBeNull();
    }

    [Test]
    public async Task ThenTheCommandIsAuditedIfItIsValid()
    {
        //Act
        await _handler.Handle(_command, CancellationToken.None);

        //Assert
        _auditService.Verify(x => x.SendAuditMessage(It.Is<AuditMessage>(c =>
            c.ImpersonatedUserEmail == AccountOwnerEmail &&
            c.ChangedProperties.SingleOrDefault(y => y.PropertyName.Equals("Role") && y.NewValue.Equals(_command.Role.ToString())) != null
        )));

        _auditService.Verify(x => x.SendAuditMessage(It.Is<AuditMessage>(c =>
            c.Description.Equals($"Member {_command.Email} on account {AccountId} role has changed to {_command.Role.ToString()}"))));

        _auditService.Verify(x => x.SendAuditMessage(It.Is<AuditMessage>(c =>
            c.RelatedEntities.SingleOrDefault(y => y.Id.Equals(AccountId.ToString()) && y.Type.Equals("Account")) != null
        )));

        _auditService.Verify(x => x.SendAuditMessage(It.Is<AuditMessage>(c =>
            c.AffectedEntity.Id.Equals(_userMembership.Id.ToString()) &&
            c.AffectedEntity.Type.Equals("Membership")
        )));
    }
}