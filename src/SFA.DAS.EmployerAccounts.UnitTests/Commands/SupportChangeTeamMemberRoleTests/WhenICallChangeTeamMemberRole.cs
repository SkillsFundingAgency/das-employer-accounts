using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.AuditCommand;
using SFA.DAS.EmployerAccounts.Commands.ChangeTeamMemberRole;
using SFA.DAS.EmployerAccounts.Commands.SupportChangeTeamMemberRole;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Models.AccountTeam;
using SFA.DAS.EmployerAccounts.Types.Models;
using SFA.DAS.Encoding;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.SupportChangeTeamMemberRoleTests;

[TestFixture]
public class WhenICallChangeTeamMemberRole
{
    private const int ExpectedAccountId = 2;
    private const string SupportUserEmail = "support@test.local";
    private const string HashedAccountId = "AJDS8HH";

    private Mock<IMembershipRepository> _membershipRepository;
    private SupportChangeTeamMemberRoleCommandHandler _handler;
    private SupportChangeTeamMemberRoleCommand _command;
    private MembershipView _callerMembership;
    private TeamMember _userMembership;
    private Mock<IMediator> _mediator;
    private Mock<IEventPublisher> _eventPublisher;
    private Mock<IEncodingService> _encodingService;

    [SetUp]
    public void Setup()
    {
        _command = new SupportChangeTeamMemberRoleCommand
        {
            HashedAccountId = HashedAccountId,
            Email = "test.user@test.local",
            Role = Role.Owner,
            SupportUserEmail = SupportUserEmail,
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

        _membershipRepository = new Mock<IMembershipRepository>();
        _membershipRepository.Setup(x => x.Get(_callerMembership.AccountId, _command.Email)).ReturnsAsync(_userMembership);

        _encodingService = new Mock<IEncodingService>();
        _encodingService.Setup(x => x.Decode(HashedAccountId, EncodingType.AccountId)).Returns(ExpectedAccountId);

        _mediator = new Mock<IMediator>();
        _eventPublisher = new Mock<IEventPublisher>();

        _handler = new SupportChangeTeamMemberRoleCommandHandler(_membershipRepository.Object, _mediator.Object, _eventPublisher.Object, _encodingService.Object);
    }
    
    [Test]
    public async Task WillChangeMembershipRole()
    {
        await _handler.Handle(_command, CancellationToken.None);

        _membershipRepository.Verify(x => x.ChangeRole(_userMembership.Id, _callerMembership.AccountId, _command.Role), Times.Once);
        _encodingService.Verify(x=> x.Decode(HashedAccountId, EncodingType.AccountId), Times.Once);
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

        Assert.That(exception.ErrorMessages.Count, Is.EqualTo(3));

        Assert.That(exception.ErrorMessages.FirstOrDefault(x => x.Key == "AccountId"), Is.Not.Null);
        Assert.That(exception.ErrorMessages.FirstOrDefault(x => x.Key == "Email"), Is.Not.Null);
        Assert.That(exception.ErrorMessages.FirstOrDefault(x => x.Key == "ExternalUserId"), Is.Not.Null);
    }

    [Test]
    public async Task ThenTheCommandIsAuditedIfItIsValid()
    {
        //Act
        await _handler.Handle(_command, CancellationToken.None);

        //Assert
        _mediator.Verify(x => x.Send(It.Is<CreateAuditCommand>(c =>
            c.EasAuditMessage.SupportUserEmail.Equals(SupportUserEmail) &&
            c.EasAuditMessage.ChangedProperties.SingleOrDefault(y => y.PropertyName.Equals("Role") && y.NewValue.Equals(_command.Role.ToString())) != null
        ), It.IsAny<CancellationToken>()));
        
        _mediator.Verify(x => x.Send(It.Is<CreateAuditCommand>(c =>
            c.EasAuditMessage.Description.Equals($"Member {_command.Email} on account {ExpectedAccountId} role has changed to {_command.Role.ToString()}")), It.IsAny<CancellationToken>()));
        
        _mediator.Verify(x => x.Send(It.Is<CreateAuditCommand>(c =>
            c.EasAuditMessage.RelatedEntities.SingleOrDefault(y => y.Id.Equals(ExpectedAccountId.ToString()) && y.Type.Equals("Account")) != null
        ), It.IsAny<CancellationToken>()));
        
        _mediator.Verify(x => x.Send(It.Is<CreateAuditCommand>(c =>
            c.EasAuditMessage.AffectedEntity.Id.Equals(_userMembership.Id.ToString()) &&
            c.EasAuditMessage.AffectedEntity.Type.Equals("Membership")
        ), It.IsAny<CancellationToken>()));
    }
}