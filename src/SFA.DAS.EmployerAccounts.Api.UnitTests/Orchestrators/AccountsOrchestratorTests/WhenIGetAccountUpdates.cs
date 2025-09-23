using AutoMapper;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Api.Orchestrators;
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

        [Test]
        public async Task ThenResponseShouldContainAllAcountsOnMultiplePages()
        {
            // Arrange
            int pageSize = 4;
            int pageNumber = 1;
            int totalExpectedPages = 5;

            DateTime toDate = DateTime.MinValue;
            var responses = new List<GetAccountsResponse>();

            for (int i = 0; i < totalExpectedPages; i++)
            {
                Accounts<AccountUpdates> accounts = new Accounts<AccountUpdates>();
                accounts.AccountList = new List<AccountUpdates>();
                var response = new GetAccountsResponse();

                for (int j = 0; j < pageSize; j++)
                {
                    accounts.AccountList.Add(new AccountUpdates
                    {
                        AccountId = j + (pageSize * i),
                        AccountName = $"Test Account {j}"
                    });
                    accounts.AccountsCount = pageSize * totalExpectedPages;
                }
                response.Accounts = accounts;
                responses.Add(response);
                pageNumber++;
            }

            _mediator.SetupSequence(x => x.Send(It.IsAny<GetAccountsQuery>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(responses[0])
                .ReturnsAsync(responses[1])
                .ReturnsAsync(responses[2])
                .ReturnsAsync(responses[3])
                .ReturnsAsync(responses[4]);

            for (int i = 0; i < totalExpectedPages; i++)
            {
                pageNumber = i + 1;
                // Act
                var result = await _orchestrator.GetAccountUpdates(toDate, pageNumber, pageSize);

                // Assert
                result.Data.Count.Should().Be(pageSize);
                result.Page.Should().Be(pageNumber);
                result.TotalPages.Should().Be(totalExpectedPages);
            }
           
        }
    }
}
