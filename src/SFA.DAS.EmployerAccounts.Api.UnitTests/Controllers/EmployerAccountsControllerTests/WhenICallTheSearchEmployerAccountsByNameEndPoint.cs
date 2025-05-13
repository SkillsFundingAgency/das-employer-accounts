using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Api.Controllers;
using SFA.DAS.EmployerAccounts.Api.Orchestrators;
using SFA.DAS.EmployerAccounts.Queries.SearchEmployerAccountsByName;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Controllers.EmployerAccountsControllerTests;

[TestFixture]
public class WhenICallTheSearchEmployerAccountsByNameEndPoint
{
    private const string ValidSearchTerm = "Test Account";
    private EmployerAccountsController _controller;
    private Mock<IMediator> _mediator;
    private Mock<ILogger<EmployerAccountsController>> _logger;
    private Mock<ILogger<AccountsOrchestrator>> _orchestratorLogger;
    private AccountsOrchestrator _orchestrator;
    private Mock<IEncodingService> _encodingService;
    private Mock<IMapper> _mapper;
    private SearchEmployerAccountsByNameResponse _response;
    private List<EmployerAccountByNameResult> _accounts;

    [SetUp]
    public void Setup()
    {
        _response = new SearchEmployerAccountsByNameResponse();
        _mapper = new Mock<IMapper>();
        _orchestratorLogger = new Mock<ILogger<AccountsOrchestrator>>();
        _mediator = new Mock<IMediator>();
        _logger = new Mock<ILogger<EmployerAccountsController>>();
      
        _encodingService = new Mock<IEncodingService>();
        _orchestrator = new AccountsOrchestrator(_mediator.Object, _orchestratorLogger.Object, _mapper.Object, _encodingService.Object);
        
        _accounts =
        [
            new()
            {
                AccountId = 1,
                DasAccountName = "Test Account 1",
                HashedAccountId = "ABC123",
                PublicHashedAccountId = "PUB123"
            },

            new()
            {
                AccountId = 2,
                DasAccountName = "Test Account 2",
                HashedAccountId = "DEF456",
                PublicHashedAccountId = "PUB456"
            }
        ];
        _response.EmployerAccounts.AddRange(_accounts);

        _mediator
            .Setup(m => m.Send(
                It.Is<SearchEmployerAccountsByNameQuery>(q => q.EmployerName == ValidSearchTerm),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_response);

        _controller = new EmployerAccountsController(
            _orchestrator,
            _encodingService.Object,
            _mediator.Object,
            _logger.Object);
    }

    [Test]
    public async Task ThenShouldReturnAccounts()
    {
        //Act
        var result = await _controller.SearchAccounts(ValidSearchTerm) as OkObjectResult;

        //Assert
        result.Should().NotBeNull();
        var model = result.Value as SearchEmployerAccountsByNameResponse;
        model.EmployerAccounts.Should().BeEquivalentTo(_accounts);
    }

    [Test]
    public async Task ThenShouldReturnEmptyListIfNoAccountsFound()
    {
        //Arrange
        _mediator.Setup(m => m.Send(It.IsAny<SearchEmployerAccountsByNameQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchEmployerAccountsByNameResponse());

        //Act
        var result = await _controller.SearchAccounts("Non Existent Employer") as OkObjectResult;

        //Assert
        result.Should().NotBeNull();
        var model = result.Value as SearchEmployerAccountsByNameResponse;
        model.EmployerAccounts.Should().BeEmpty();
    }

    [Test]
    public async Task ThenShouldReturnBadRequestIfEmployerNameIsEmpty()
    {
        //Act
        var result = await _controller.SearchAccounts(string.Empty) as BadRequestResult;

        //Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(400);
    }

    [Test]
    public async Task ThenShouldReturnBadRequestIfEmployerNameIsNull()
    {
        //Act
        var result = await _controller.SearchAccounts(null) as BadRequestResult;

        //Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(400);
    }
} 