﻿using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Api.Controllers;
using SFA.DAS.EmployerAccounts.Api.Orchestrators;
using SFA.DAS.EmployerAccounts.Commands.ChangeTeamMemberRole;
using SFA.DAS.EmployerAccounts.Models;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Controllers.EmployerUserControllerTests;

public class WhenICallTheChangeRoleEndpoint
{
    private EmployerUserController _controller;
    private Mock<IMediator> _mediator;
    private ChangeTeamMemberRoleCommand _command;

    [SetUp]
    public void Setup()
    {
        _mediator = new Mock<IMediator>();

        _command = new ChangeTeamMemberRoleCommand
        {
            HashedAccountId = "ABC123",
            Email = "test@email.test",
            ExternalUserId = Guid.NewGuid().ToString(),
            Role = Role.Owner,
        };

        _controller = new EmployerUserController(Mock.Of<IUsersOrchestrator>(), _mediator.Object, Mock.Of<ILogger<EmployerUserController>>());
    }

    [Test]
    public async Task ThenTheCommandShouldBeSent()
    {
        var response = await _controller.ChangeRole(_command);

        response.Should().BeOfType<OkResult>();

        _mediator.Verify(x => x.Send(_command, new CancellationToken()), Times.Once);
    }

    [Test]
    public async Task ThenInternalServerErrorIsReturnedWhenThereIsAnException()
    {
        _mediator.Setup(x => x.Send(It.Is<ChangeTeamMemberRoleCommand>(y => y == _command), new CancellationToken())).Throws<Exception>();

        var response = await _controller.ChangeRole(_command);
        
        ((StatusCodeResult)response).StatusCode.Should().Be(500);
    }
}