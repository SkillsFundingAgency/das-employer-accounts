using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Api.Orchestrators;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAccountDetail;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Orchestrators.AccountsOrchestratorTests
{
    internal class WhenIGetAnAccount
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

            _mediator
                .Setup(x => x.Send(It.IsAny<GetEmployerAccountDetailByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetEmployerAccountDetailByIdResponse())
                .Verifiable("Get account was not called");
        }

        [Test]
        public async Task TheARequestToGetAccountDetailsShouldBeMade()
        {
            //Arrange
            const long accountId = 9911;

            //Act
            await _orchestrator.GetAccount(accountId);

            //Assert
            _mediator.VerifyAll();
        }
    }
}
