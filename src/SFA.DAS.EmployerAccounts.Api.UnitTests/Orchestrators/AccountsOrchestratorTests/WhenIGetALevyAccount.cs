using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Api.Orchestrators;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAccountDetail;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Orchestrators.AccountsOrchestratorTests
{
    internal class WhenIGetALevyAccount 
    {
        private AccountsOrchestrator _orchestrator;
        private Mock<IMediator> _mediator;
        private Mock<ILogger<AccountsOrchestrator>> _log;
      
        [SetUp]
        public void Arrange()
        {
            _mediator = new Mock<IMediator>();
  
            _log = new Mock<ILogger<AccountsOrchestrator>>();
          
            _orchestrator = new AccountsOrchestrator(_mediator.Object, _log.Object, Mock.Of<IMapper>(), Mock.Of<IEncodingService>());

            var response = new GetEmployerAccountDetailByIdResponse
            {
                Account = new AccountDetail
                {
                    AccountAgreementTypes = new List<AgreementType>()
                    {
                        AgreementType.Levy
                    }
                }
            };

            _mediator
                .Setup(x => x.Send(It.IsAny<GetEmployerAccountDetailByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response)
                .Verifiable("Get account was not called");
        }

        [Test]
        public async Task ThenResponseShouldHaveAccountAgreementTypeSetToLevy()
        {
            //Arrange
            AgreementType agreementType = AgreementType.Levy;
            const long accountId = 999;

            //Act
            var result = await _orchestrator.GetAccount(accountId);

            //Assert
            result.AccountAgreementType.ToString().Should().Be(agreementType.ToString());
        }        
    }
}
