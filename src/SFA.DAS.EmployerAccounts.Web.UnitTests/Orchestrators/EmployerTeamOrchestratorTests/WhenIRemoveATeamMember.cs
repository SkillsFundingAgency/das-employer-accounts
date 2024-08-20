using System;
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
using SFA.DAS.EmployerAccounts.Commands.RemoveTeamMember;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Exceptions;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Models.AccountTeam;
using SFA.DAS.EmployerAccounts.Models.UserProfile;
using SFA.DAS.EmployerAccounts.Queries.GetAccountTeamMembers;
using SFA.DAS.EmployerAccounts.Queries.GetUser;
using SFA.DAS.EmployerAccounts.Web.Orchestrators;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Orchestrators.EmployerTeamOrchestratorTests;

class WhenIRemoveATeamMember
{
    const string Email = "test@test.com";

    private Mock<IMediator> _mediator;
    private Mock<IAccountApiClient> _accountApiClient;        
    private Mock<IMapper> _mapper;
    private Mock<IEncodingService> _encodingService;
    private const long UserId = 2;
    private const string HashedUserId = "DSHD23D";
    private EmployerTeamOrchestrator _orchestrator;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _accountApiClient = new Mock<IAccountApiClient>();            
        _mapper = new Mock<IMapper>();
        _encodingService = new Mock<IEncodingService>();

        _encodingService.Setup(x => x.Decode(HashedUserId, EncodingType.AccountId)).Returns(UserId);

        _orchestrator = new EmployerTeamOrchestrator(_mediator.Object, Mock.Of<ICurrentDateTime>(), _accountApiClient.Object, _mapper.Object, Mock.Of<EmployerAccountsConfiguration>(), _encodingService.Object);
            
        _mediator.Setup(x => x.Send(It.IsAny<GetAccountTeamMembersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetAccountTeamMembersResponse
            {
                TeamMembers = new List<TeamMember>()
            });

        _mediator.Setup(x => x.Send(It.IsAny<GetUserQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new GetUserResponse
        {
            User = new User
            {
                Email = Email,
                UserRef = Guid.NewGuid().ToString()
            }
        });
    }

    [Test]
    public async Task ThenIShouldGetASuccessMessage()
    {
        //Arrange

        //Act
        var result = await _orchestrator.Remove( "3242", "32342", HashedUserId);

        //Assert
        result.Status.Should().Be(HttpStatusCode.OK);
        result.FlashMessage.Headline.Should().Be("Team member removed");
        result.FlashMessage.Message.Should().Be($"You've removed <strong>{Email}</strong>");
    }

    [Test]
    public async Task ThenIShouldGetANotFoundErrorMessageIfNoUserCanBeFound()
    {
        //Arrange
        _mediator.Setup(x => x.Send(It.IsAny<GetUserQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(new GetUserResponse());

        //Act
        var result = await _orchestrator.Remove( "3242", "32342", HashedUserId);

        //Assert
        result.Status.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task ThenIShouldGetAInvalidRequestErrorMessageIfExceptionIsThrow()
    {
        //Arrange
        _mediator.Setup(x => x.Send(It.IsAny<RemoveTeamMemberCommand>(), It.IsAny<CancellationToken>())).Throws(new InvalidRequestException(new Dictionary<string, string>()));

        //Act
        var result = await _orchestrator.Remove( "3242", "32342", HashedUserId);

        //Assert
        result.Status.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ThenIShouldGetAUnauthorisedErrorMessageIfExceptionIsThrow()
    {
        //Arrange
        _mediator.Setup(x => x.Send(It.IsAny<RemoveTeamMemberCommand>(), It.IsAny<CancellationToken>())).Throws<UnauthorizedAccessException>();

        //Act
        var result = await _orchestrator.Remove("3242", "32342", HashedUserId);

        //Assert
        result.Status.Should().Be(HttpStatusCode.Unauthorized);
    }
}