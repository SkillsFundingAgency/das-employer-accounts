using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.ResendInvitation;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Controllers.EmployerTeamController;

public class WhenICallTheResendInvitationEndpoint
{
    private Api.Controllers.EmployerTeamController _controller;
    private Mock<IMediator> _mediator;
    private ResendInvitationCommand _command;

    [SetUp]
    public void Setup()
    {
        _mediator = new Mock<IMediator>();

        _command = new ResendInvitationCommand
        {
            HashedAccountId = "ABC123",
            Email = "test@email.test",
            ExternalUserId = Guid.NewGuid().ToString(),
            FirstName = "User",
            };

        _controller = new Api.Controllers.EmployerTeamController(_mediator.Object, Mock.Of<ILogger<Api.Controllers.EmployerTeamController>>());
    }

    [Test]
    public async Task ThenTheCommandShouldBeSent()
    {
        var response = await _controller.ResendInvitation(_command);

        response.Should().BeOfType<OkResult>();

        _mediator.Verify(x => x.Send(_command, new CancellationToken()), Times.Once);
    }
    
    [Test]
    public async Task ThenInternalServerErrorIsReturnedWhenThereIsAnException()
    {
        _mediator.Setup(x => x.Send(It.Is<ResendInvitationCommand>(y => y == _command), new CancellationToken())).Throws<Exception>();

        var response = await _controller.ResendInvitation(_command);

        ((StatusCodeResult)response).StatusCode.Should().Be(500);
    }
}