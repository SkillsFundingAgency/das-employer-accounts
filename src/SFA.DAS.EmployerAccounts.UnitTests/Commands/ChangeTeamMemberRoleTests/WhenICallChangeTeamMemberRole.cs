using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elasticsearch.Net;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Audit.Types;
using SFA.DAS.EmployerAccounts.Commands.AuditCommand;
using SFA.DAS.EmployerAccounts.Commands.ChangeTeamMemberRole;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Models.AccountTeam;
using SFA.DAS.EmployerAccounts.Types.Models;
using SFA.DAS.Encoding;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.ChangeTeamMemberRoleTests;

[TestFixture]
public class WhenICallChangeTeamMemberRole
{
    private const int ExpectedAccountId = 2;
    private Mock<IMembershipRepository> _membershipRepository;
    private ChangeTeamMemberRoleCommandHandler _handler;
    private ChangeTeamMemberRoleCommand _command;
    private MembershipView _callerMembership;
    private TeamMember _userMembership;
    private Mock<IEventPublisher> _eventPublisher;
    private Mock<IEncodingService> _encodingService;
    private Mock<IAuditService> _auditService;

    [SetUp]
    public void Setup()
    {
        _command = new ChangeTeamMemberRoleCommand
        {
            HashedAccountId = "1",
            HashedUserId = "ADSVD",
            Role = Role.Owner,
            ExternalUserId = Guid.NewGuid().ToString(),
        };

        _callerMembership = new MembershipView
        {
            AccountId = ExpectedAccountId,
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

        _encodingService = new Mock<IEncodingService>();
        _encodingService.Setup(x => x.Decode(_command.HashedUserId, EncodingType.AccountId)).Returns(_userMembership.Id);

        _membershipRepository = new Mock<IMembershipRepository>();
        _membershipRepository.Setup(x => x.GetCaller(_command.HashedAccountId, _command.ExternalUserId)).ReturnsAsync(_callerMembership);
        _membershipRepository.Setup(x => x.Get(_callerMembership.AccountId, _userMembership.Id)).ReturnsAsync(_userMembership);

        _eventPublisher = new Mock<IEventPublisher>();
        _auditService = new Mock<IAuditService>();

        _handler = new ChangeTeamMemberRoleCommandHandler(_membershipRepository.Object, _eventPublisher.Object, _encodingService.Object, _auditService.Object);
    }

    [Test]
    public void IfCallerNotMemberOfAccountWillThrowException()
    {
        var command = new ChangeTeamMemberRoleCommand
        {
            HashedAccountId = "1",
            HashedUserId = "ADSDSVD",
            Role = Role.Owner,
            ExternalUserId = Guid.NewGuid().ToString()
        };

        _membershipRepository.Setup(x => x.GetCaller(command.HashedAccountId, command.ExternalUserId)).ReturnsAsync(() => null);
        
        var action = () => _handler.Handle(command, CancellationToken.None);

        action.Should().ThrowAsync<InvalidRequestException>()
            .WithMessage("You are not a member of this Account");
    }

    [Test]
    public void IfCallerNotAccountOwnerWillThrowException()
    {
        var command = new ChangeTeamMemberRoleCommand
        {
            HashedAccountId = "1",
            HashedUserId = "WDFES",
            Role = Role.Owner,
            ExternalUserId = Guid.NewGuid().ToString()
        };

        var callerMembership = new MembershipView
        {
            AccountId = ExpectedAccountId,
            UserId = 1,
            Role = Role.Viewer
        };

        _membershipRepository.Setup(x => x.GetCaller(callerMembership.AccountId, command.ExternalUserId)).ReturnsAsync(callerMembership);

        var action = () => _handler.Handle(command, CancellationToken.None);
        
        action.Should().ThrowAsync<InvalidRequestException>()
            .WithMessage("You must be an owner of this Account");
    }

    [Test]
    public void IfUserNotMemberOfAccountWillThrowException()
    {
        var command = new ChangeTeamMemberRoleCommand
        {
            HashedAccountId = "1",
            HashedUserId = "ADSAWDVD",
            Role = Role.Owner,
            ExternalUserId = Guid.NewGuid().ToString()
        };

        var callerMembership = new MembershipView
        {
            AccountId = ExpectedAccountId,
            UserId = 1,
            Role = Role.Owner
        };

        _membershipRepository.Setup(x => x.GetCaller(callerMembership.AccountId, command.ExternalUserId)).ReturnsAsync(callerMembership);
        _membershipRepository.Setup(x => x.Get(callerMembership.AccountId, command.HashedUserId)).ReturnsAsync(() => null);
        
        var action = () => _handler.Handle(command, CancellationToken.None);

        action.Should().ThrowAsync<InvalidRequestException>()
            .WithMessage("You are not a member of this Account");
    }

    [Test]
    public void IfUserIsCallerWillThrowException()
    {
        var command = new ChangeTeamMemberRoleCommand
        {
            HashedAccountId = "1",
            HashedUserId = "WSSVDWQ",
            Role = Role.Owner,
            ExternalUserId = Guid.NewGuid().ToString()
        };

        var callerMembership = new MembershipView
        {
            AccountId = ExpectedAccountId,
            UserId = 1,
            Role = Role.Owner
        };

        var userMembership = new TeamMember
        {
            AccountId = callerMembership.AccountId,
            Id = callerMembership.UserId,
            Role = callerMembership.Role
        };

        _membershipRepository.Setup(x => x.GetCaller(callerMembership.AccountId, command.ExternalUserId)).ReturnsAsync(callerMembership);
        _membershipRepository.Setup(x => x.Get(callerMembership.AccountId, command.HashedUserId)).ReturnsAsync(userMembership);

        var action = () => _handler.Handle(command, CancellationToken.None);

        action.Should().ThrowAsync<InvalidRequestException>()
            .WithMessage("You cannot change your own role");
    }

    [Test]
    public async Task WillChangeMembershipRole()
    {
        await _handler.Handle(_command, CancellationToken.None);

        _membershipRepository.Verify(x => x.ChangeRole(_userMembership.Id, _callerMembership.AccountId, _command.Role), Times.Once);
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
        var command = new ChangeTeamMemberRoleCommand();

        var action = () => _handler.Handle(command, CancellationToken.None);

        action.Should().ThrowAsync<InvalidRequestException>()
            .WithMessage("No HashedId supplied")
            .WithMessage("No HashedUserId supplied")
            .WithMessage("No ExternalUserId supplied");
    }

    [Test]
    public async Task ThenTheTheCommandIsAuditedIfItIsValid()
    {
        //Act
        await _handler.Handle(_command, CancellationToken.None);

        //Assert
        _auditService.Verify(x => x.SendAuditMessage(It.Is<AuditMessage>(c =>
            c.ChangedProperties.SingleOrDefault(y => y.PropertyName.Equals("Role") && y.NewValue.Equals(_command.Role.ToString())) != null 
            && c.Description.Equals($"Member {_userMembership.Email} on account {ExpectedAccountId} role has changed to {_command.Role.ToString()}")
            && c.RelatedEntities.SingleOrDefault(y => y.Id.Equals(ExpectedAccountId.ToString()) && y.Type.Equals("Account")) != null
            && c.AffectedEntity.Id.Equals(_userMembership.Id.ToString()) 
            && c.AffectedEntity.Type.Equals("Membership")
        )));
    }
}