using AutoMapper;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Api.Orchestrators;
using SFA.DAS.EmployerAccounts.Api.Types;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Queries.GetAccounts;
using SFA.DAS.Encoding;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Orchestrators.AccountsOrchestratorTests
{
    public class WhenIGetAccountUpdates
    {
        private AccountsOrchestrator _orchestrator;
        private Mock<IMediator> _mediator;
        private Mock<ILogger<AccountsOrchestrator>> _log;

        [SetUp]
        public void Setup()
        {
            _mediator = new Mock<IMediator>();
            _log = new Mock<ILogger<AccountsOrchestrator>>();
            _orchestrator = new AccountsOrchestrator(_mediator.Object, _log.Object, Mock.Of<IMapper>(), Mock.Of<IEncodingService>());

            
        }

        [Test]
        public async Task ThenResponseShouldContainAllAcountsOnASinglePage()
        {
            // Arrange
            int pageSize = 1000;
            int pageNumber = 1;
            
            DateTime toDate = DateTime.MinValue;

            var response = new GetAccountsResponse
            {
                Accounts = new Accounts<AccountUpdates>()
                {
                    AccountList = new List<AccountUpdates>
                    {
                        new AccountUpdates { AccountId = 1, AccountName = "Test Account 1" },
                        new AccountUpdates { AccountId = 2, AccountName = "Test Account 2" }
                    },
                    AccountsCount = 2
                }
            };

            _mediator
                .Setup(x => x.Send(It.IsAny<GetAccountsQuery>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(response)
                .Verifiable("Get accounts was not called");

            // Act
            var result = await _orchestrator.GetAccountUpdates(toDate, pageNumber, pageSize);

            // Assert
            result.Data.Count.Should().Be(2);
            result.Page.Should().Be(pageNumber);
            result.TotalPages.Should().Be(1);
        }

        [TestCase(1, 4, Description = "Get First Page")]
        [TestCase(2, 4, Description = "Get Second Page")]
        [TestCase(3, 4, Description = "Get Third Page")]
        [TestCase(4, 4, Description = "Get Fourth Page")]
        [TestCase(5, 4, Description = "Get Fifth Page")]
        public async Task ThenResponseShouldContainAllAcountsOnMultiplePages(int pageNumber, int pageSize)
        {
            // Arrange
            int totalExpectedPages = 5;
            int offset = pageSize * pageNumber;

            DateTime toDate = DateTime.MinValue;
            Accounts<AccountUpdates> accounts = new Accounts<AccountUpdates>();
            accounts.AccountList = new List<AccountUpdates>();
            var response = new GetAccountsResponse();

            for (int j = 0; j < pageSize; j++)
            {
                accounts.AccountList.Add(new AccountUpdates
                {
                    AccountId = j + offset,
                    AccountName = $"Test Account {j + offset}"
                });
                accounts.AccountsCount = pageSize * totalExpectedPages;
            }
            response.Accounts = accounts;

            _mediator.SetupSequence(x => x.Send(It.IsAny<GetAccountsQuery>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _orchestrator.GetAccountUpdates(toDate, pageNumber, pageSize);

            // Assert
            result.Data.Count.Should().Be(pageSize);
            result.Page.Should().Be(pageNumber);
            result.TotalPages.Should().Be(totalExpectedPages);

            for(int j = 0; j < pageSize; j++)
            {
                result.Data[j].AccountId.Should().Be(j + offset);
                result.Data[j].AccountName.Should().Be($"Test Account {j + offset}");
            }
        }
    }
}
