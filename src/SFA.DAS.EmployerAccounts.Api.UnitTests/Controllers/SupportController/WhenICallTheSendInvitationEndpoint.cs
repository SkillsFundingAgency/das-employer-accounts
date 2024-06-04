using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.ChangeTeamMemberRole;
using SFA.DAS.EmployerAccounts.Commands.SupportChangeTeamMemberRole;
using SFA.DAS.EmployerAccounts.Commands.SupportCreateInvitation;
using SFA.DAS.EmployerAccounts.Models;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Controllers.SupportController;

public class WhenICallTheSendInvitationEndpoint
{
    private Api.Controllers.SupportController _controller;
    private Mock<IMediator> _mediator;
    private SupportCreateInvitationCommand _command;

    [SetUp]
    public void Setup()
    {
        _mediator = new Mock<IMediator>();

        _command = new SupportCreateInvitationCommand
        {
            HashedAccountId = "ABC123",
            EmailOfPersonBeingInvited = "test@email.test",
            NameOfPersonBeingInvited = "Test User",
            RoleOfPersonBeingInvited = Role.Owner,
            SupportUserEmail = "support@user.test"
        };

        _controller = new Api.Controllers.SupportController( _mediator.Object, Mock.Of<ILogger<Api.Controllers.SupportController>>());
    }

    [Test]
    public async Task ThenTheCommandShouldBeSent()
    {
        var response = await _controller.SendInvitation(_command);

        response.Should().BeOfType<OkResult>();

        _mediator.Verify(x => x.Send(_command, new CancellationToken()), Times.Once);
    }

    [Test]
    public async Task ThenInternalServerErrorIsReturnedWhenThereIsAnException()
    {
        _mediator.Setup(x => x.Send(It.Is<SupportCreateInvitationCommand>(y => y == _command), new CancellationToken())).Throws<Exception>();

        var response = await _controller.SendInvitation(_command);
        
        ((StatusCodeResult)response).StatusCode.Should().Be(500);
    }
}