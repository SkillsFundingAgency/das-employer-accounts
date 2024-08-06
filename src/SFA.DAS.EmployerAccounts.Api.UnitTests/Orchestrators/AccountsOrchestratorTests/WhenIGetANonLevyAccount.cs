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

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Orchestrators.AccountsOrchestratorTests;

internal class WhenIGetANonLevyAccount
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
    }

    [Test]
    public async Task ThenResponseShouldHaveAccountAgreementTypeSetToCombined()
    {
        //Arrange
        var agreementType = AgreementType.Combined;
        const long accountId = 22334;

        var response = new GetEmployerAccountDetailByIdResponse
        {
            Account = new AccountDetail
            {
                AccountAgreementTypes = new List<AgreementType>
                    {
                        agreementType,
                        agreementType
                    }
            }
        };

        _mediator
            .Setup(x => x.Send(It.IsAny<GetEmployerAccountDetailByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response)
            .Verifiable("Get account was not called");

        //Act
        var result = await _orchestrator.GetAccount(accountId);

        //Assert
        _mediator.Verify();
        _mediator.VerifyNoOtherCalls();
        result.AccountAgreementType.ToString().Should().Be(agreementType.ToString());
    }
}

