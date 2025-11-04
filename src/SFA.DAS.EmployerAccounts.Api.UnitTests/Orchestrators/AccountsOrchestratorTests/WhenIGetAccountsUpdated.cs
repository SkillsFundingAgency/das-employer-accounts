using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Api.Orchestrators;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Queries.GetAccountsSinceDate;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Orchestrators.AccountsOrchestratorTests;

[TestFixture]
public class WhenIGetAccountsUpdated
{
    private AccountsOrchestrator _sut;
    private Mock<IMediator> _mediator;
    private Mock<ILogger<AccountsOrchestrator>> _logger;

    [SetUp]
    public void Setup()
    {
        _mediator = new Mock<IMediator>();
        _logger = new Mock<ILogger<AccountsOrchestrator>>();
        _sut = new AccountsOrchestrator(_mediator.Object, _logger.Object, Mock.Of<IMapper>());
    }

    [Test]
    public async Task Then_Response_Should_Contain_All_Accounts_On_A_Single_Page()
    {
        var pageSize = 1000;
        var pageNumber = 1;
        var sinceDate = DateTime.MinValue;

        var response = new GetAccountsSinceDateResponse
        {
            Accounts = new Accounts<AccountNameSummary>
            {
                AccountList = new List<AccountNameSummary>
                {
                    new() { Id = 1, Name = "Test Account 1" },
                    new() { Id = 2, Name = "Test Account 2" }
                },
                AccountsCount = 2
            }
        };

        _mediator
            .Setup(x => x.Send(It.IsAny<GetAccountsSinceDateQuery>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(response)
            .Verifiable();

        var result = await _sut.GetAccountsUpdated(sinceDate, pageNumber, pageSize);

        result.Data.Should().HaveCount(2);
        result.Page.Should().Be(pageNumber);
        result.TotalPages.Should().Be(1);

        _mediator.Verify(x => x.Send(It.IsAny<GetAccountsSinceDateQuery>(), It.IsAny<System.Threading.CancellationToken>()), Times.Once);
    }

    [TestCase(1, 4, Description = "First Page")]
    [TestCase(2, 4, Description = "Second Page")]
    [TestCase(3, 4, Description = "Third Page")]
    [TestCase(4, 4, Description = "Fourth Page")]
    [TestCase(5, 4, Description = "Fifth Page")]
    public async Task Then_Response_Should_Contain_All_Accounts_On_Multiple_Pages(int pageNumber, int pageSize)
    {
        int totalExpectedPages = 5;
        int offset = pageSize * pageNumber;
        var sinceDate = DateTime.MinValue;

        var accounts = new Accounts<AccountNameSummary>
        {
            AccountList = new List<AccountNameSummary>(),
            AccountsCount = pageSize * totalExpectedPages
        };

        for (int j = 0; j < pageSize; j++)
        {
            accounts.AccountList.Add(new AccountNameSummary
            {
                Id = j + offset,
                Name = $"Test Account {j + offset}"
            });
        }

        var response = new GetAccountsSinceDateResponse { Accounts = accounts };

        _mediator
            .Setup(x => x.Send(It.IsAny<GetAccountsSinceDateQuery>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _sut.GetAccountsUpdated(sinceDate, pageNumber, pageSize);

        result.Data.Count.Should().Be(pageSize);
        result.Page.Should().Be(pageNumber);
        result.TotalPages.Should().Be(totalExpectedPages);

        for (int j = 0; j < pageSize; j++)
        {
            result.Data[j].AccountId.Should().Be(j + offset);
            result.Data[j].AccountName.Should().Be($"Test Account {j + offset}");
        }
    }

    [Test]
    public async Task Then_Paging_Should_Use_Ceiling_For_Remaining_Accounts()
    {
        var sinceDate = DateTime.MinValue;
        var pageSize = 3;
        var pageNumber = 1;

        var response = new GetAccountsSinceDateResponse
        {
            Accounts = new Accounts<AccountNameSummary>
            {
                AccountList = new List<AccountNameSummary>
                {
                    new() { Id = 1, Name = "A" },
                    new() { Id = 2, Name = "B" }
                },
                AccountsCount = 5
            }
        };

        _mediator
            .Setup(x => x.Send(It.IsAny<GetAccountsSinceDateQuery>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _sut.GetAccountsUpdated(sinceDate, pageNumber, pageSize);

        result.TotalPages.Should().Be(2);
    }

    [Test]
    public async Task Then_If_No_Accounts_Then_Returns_Empty_List_And_One_Page()
    {
        var response = new GetAccountsSinceDateResponse
        {
            Accounts = new Accounts<AccountNameSummary>
            {
                AccountList = new List<AccountNameSummary>(),
                AccountsCount = 0
            }
        };

        _mediator
            .Setup(x => x.Send(It.IsAny<GetAccountsSinceDateQuery>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _sut.GetAccountsUpdated(DateTime.MinValue, 1, 10);

        result.Data.Should().BeEmpty();
        result.TotalPages.Should().Be(1);
        result.Page.Should().Be(1);
    }

    [Test]
    public async Task Then_Logs_Information_When_Called()
    {
        var response = new GetAccountsSinceDateResponse
        {
            Accounts = new Accounts<AccountNameSummary>
            {
                AccountList = new List<AccountNameSummary>(),
                AccountsCount = 1
            }
        };

        _mediator
            .Setup(x => x.Send(It.IsAny<GetAccountsSinceDateQuery>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(response);

        await _sut.GetAccountsUpdated(DateTime.MinValue, 1, 10);

        _logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("Getting accounts updated since")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}