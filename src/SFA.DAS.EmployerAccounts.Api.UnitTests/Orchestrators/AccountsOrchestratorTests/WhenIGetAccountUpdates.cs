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
        }

        [Test]
        public async Task ThenResponseShouldContainAllAcountsOnASinglePage()
        {
            // Arrange
            int pageSize = 1000;
            int pageNumber = 1;
            DateTime toDate = DateTime.MinValue;
            
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
            int pageSize = 1;
            int pageNumber = 2;
            DateTime toDate = DateTime.MinValue;
         
            // Act
            var result = await _orchestrator.GetAccountUpdates(toDate, pageNumber, pageSize);
            
            // Assert
            result.Data.Count.Should().Be(2);
            result.Page.Should().Be(pageNumber);
            result.TotalPages.Should().Be(2);
        }
    }
}
