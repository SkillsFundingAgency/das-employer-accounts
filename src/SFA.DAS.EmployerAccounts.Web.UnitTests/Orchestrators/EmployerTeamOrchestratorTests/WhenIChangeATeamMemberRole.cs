﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EmployerAccounts.Commands.ChangeTeamMemberRole;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Exceptions;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Queries.GetAccountTeamMembers;
using SFA.DAS.EmployerAccounts.Web.Orchestrators;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Orchestrators.EmployerTeamOrchestratorTests;

public class WhenIChangeATeamMemberRole
{
    private Mock<IMediator> _mediator;
    private Mock<IAccountApiClient> _accountApiClient;
    private Mock<IMapper> _mapper;
    private EmployerTeamOrchestrator _orchestrator;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _accountApiClient = new Mock<IAccountApiClient>();
        _mapper = new Mock<IMapper>();

        _orchestrator = new EmployerTeamOrchestrator(_mediator.Object, Mock.Of<ICurrentDateTime>(), _accountApiClient.Object, _mapper.Object, Mock.Of<EmployerAccountsConfiguration>(), Mock.Of<IEncodingService>());
    }

    [Test]
    public async Task ThenIShouldGetBackAnUpdatedTeamMembersListWithTheCorrectSuccessMessage()
    {
        //Assign
        var hashedUserId = Guid.NewGuid().ToString();
        const string email = "test@test.com";
        const Role role = Role.Owner;
        var response = new GetAccountTeamMembersResponse();
        _mediator.Setup(x => x.Send(It.IsAny<GetAccountTeamMembersQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);

        //Act
        var result = await _orchestrator.ChangeRole("437675", hashedUserId, role, "37648", email);

        //Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(HttpStatusCode.OK);
        result.FlashMessage.Should().NotBeNull();
        result.FlashMessage.Headline.Should().Be("Team member updated");
        result.FlashMessage.Message.Should().Be($"{email} can now {RoleStrings.GetRoleDescriptionToLower(role)}");
    }

    [Test]
    public async Task ThenIShouldGetBackABadRequestIfOneIsRaised()
    {
        //Assign
        var hashedUserId = Guid.NewGuid().ToString();
        const string email = "test@test.com";
        const Role role = Role.Owner;

        _mediator.Setup(x => x.Send(It.IsAny<ChangeTeamMemberRoleCommand>(), It.IsAny<CancellationToken>()))
            .Throws(new InvalidRequestException(new Dictionary<string, string>()));

        //Act
        var result = await _orchestrator.ChangeRole("437675", hashedUserId, role, "37648", email);


        //Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(HttpStatusCode.BadRequest);
        _mediator.Verify(x => x.Send(It.IsAny<GetAccountTeamMembersQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ThenIShouldGetBackAnUnauthorisedRequestIfOneIsRaised()
    {
        //Assign
        var hashedUserId = Guid.NewGuid().ToString();
        const string email = "test@test.com";
        const Role role = Role.Owner;

        _mediator.Setup(x => x.Send(It.IsAny<ChangeTeamMemberRoleCommand>(), It.IsAny<CancellationToken>()))
            .Throws(new UnauthorizedAccessException());

        //Act
        var result = await _orchestrator.ChangeRole("437675", hashedUserId, role, "37648", email);

        //Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(HttpStatusCode.Unauthorized);
        _mediator.Verify(x => x.Send(It.IsAny<GetAccountTeamMembersQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}