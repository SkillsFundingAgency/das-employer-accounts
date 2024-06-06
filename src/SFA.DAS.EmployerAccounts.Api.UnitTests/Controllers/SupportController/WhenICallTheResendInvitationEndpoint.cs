using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.SupportResendInvitationCommand;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Controllers.SupportController;

public class WhenICallTheResendInvitationEndpoint
{
    private Api.Controllers.SupportController _controller;
    private Mock<IMediator> _mediator;
    private SupportResendInvitationCommand _command;

    [SetUp]
    public void Setup()
    {
        _mediator = new Mock<IMediator>();

        _command = new SupportResendInvitationCommand
        {
            HashedAccountId = "ABC123",
            Email = "test@email.test",
            };

        _controller = new Api.Controllers.SupportController(_mediator.Object, Mock.Of<ILogger<Api.Controllers.SupportController>>());
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
        _mediator.Setup(x => x.Send(It.Is<SupportResendInvitationCommand>(y => y == _command), new CancellationToken())).Throws<Exception>();

        var response = await _controller.ResendInvitation(_command);

        ((StatusCodeResult)response).StatusCode.Should().Be(500);
    }
}