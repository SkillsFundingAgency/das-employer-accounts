using System.Collections.Generic;
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
using SFA.DAS.EmployerAccounts.Queries.SearchEmployerAccountsByName;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Controllers.EmployerAccountsControllerTests;

[TestFixture]
public class WhenICallTheSearchEmployerAccountsByNameEndPoint
{
    private EmployerAccountsController _controller;
    private Mock<IMediator> _mediator;
    private Mock<ILogger<EmployerAccountsController>> _logger;
    private Mock<AccountsOrchestrator> _orchestrator;
    private Mock<IEncodingService> _encodingService;
    private List<EmployerAccountByNameResult> _accounts;

    [SetUp]
    public void Setup()
    {
        _mediator = new Mock<IMediator>();
        _logger = new Mock<ILogger<EmployerAccountsController>>();
        _orchestrator = new Mock<AccountsOrchestrator>();
        _encodingService = new Mock<IEncodingService>();

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

        _mediator.Setup(m => m.Send(It.IsAny<SearchEmployerAccountsByNameQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_accounts);

        _controller = new EmployerAccountsController(
            _orchestrator.Object,
            _encodingService.Object,
            _mediator.Object,
            _logger.Object);
    }

    [Test]
    public async Task ThenShouldReturnAccounts()
    {
        //Act
        var result = await _controller.SearchAccounts("Test Employer") as OkObjectResult;

        //Assert
        result.Should().NotBeNull();
        var model = result.Value as List<EmployerAccountByNameResult>;
        model.Should().BeEquivalentTo(_accounts);
    }

    [Test]
    public async Task ThenShouldReturnEmptyListIfNoAccountsFound()
    {
        //Arrange
        _mediator.Setup(m => m.Send(It.IsAny<SearchEmployerAccountsByNameQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EmployerAccountByNameResult>());

        //Act
        var result = await _controller.SearchAccounts("Non Existent Employer") as OkObjectResult;

        //Assert
        result.Should().NotBeNull();
        var model = result.Value as List<EmployerAccountByNameResult>;
        model.Should().BeEmpty();
    }

    [Test]
    public async Task ThenShouldReturnBadRequestIfEmployerNameIsEmpty()
    {
        //Act
        var result = await _controller.SearchAccounts(string.Empty) as BadRequestObjectResult;

        //Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(400);
    }

    [Test]
    public async Task ThenShouldReturnBadRequestIfEmployerNameIsNull()
    {
        //Act
        var result = await _controller.SearchAccounts(null) as BadRequestObjectResult;

        //Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(400);
    }
} 