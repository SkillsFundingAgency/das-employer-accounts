using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Api.Orchestrators;
using SFA.DAS.EmployerAccounts.Queries.GetAccountById;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Orchestrators.AccountsOrchestratorTests
{
    public class WhenIGetAccountById
    {

        [Test, MoqAutoData]
        public async Task AndResponseFromMediator_IsNot_Null_Response_IsNot_Null(
            long accountId,
            [Frozen] Mock<IMediator> _mediator,
            AccountsOrchestrator _orchestrator
            )
        {
            var response = new GetAccountByIdResponse
            {
                Account = new Models.Account.Account()
            };

            _mediator
                .Setup(m => m.Send(It.Is<GetAccountByIdQuery>(r => r.AccountId == accountId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _orchestrator.GetAccountById(accountId);

            Assert.IsNotNull(result);
        }

        [Test, MoqAutoData]
        public async Task AndResponseFromMediator_Is_Null_Response_Is_Null(
           long accountId,
           [Frozen] Mock<IMediator> _mediator,
           AccountsOrchestrator _orchestrator
           )
        {
            var response = new GetAccountByIdResponse
            {
                Account = null
            };
            _mediator
                .Setup(m => m.Send(It.Is<GetAccountByIdQuery>(r => r.AccountId == accountId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _orchestrator.GetAccountById(accountId);

            Assert.IsNull(result);
        }
    }
}