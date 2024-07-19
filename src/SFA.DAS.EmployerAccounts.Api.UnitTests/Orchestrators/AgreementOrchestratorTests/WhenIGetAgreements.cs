using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using AutoMapper;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Api.Orchestrators;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAgreementsByAccountId;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Orchestrators.AgreementOrchestratorTests;

public class WhenIGetAgreements
{
    [Test, MoqAutoData]
    public async Task ThenAnEmptyResponseShouldBeReturnedWhenThereAreNoAgreements(
        long accountId,
        [Frozen] Mock<ILogger<AgreementOrchestrator>> logger,
        [Frozen] Mock<IMediator> mediator,
        [Frozen] Mock<IMapper> mapper
        )
    {
        var sut = new AgreementOrchestrator(mediator.Object, logger.Object, mapper.Object);
        var result = await sut.GetAgreements(accountId);

        result.Should().NotBeNull();
        result.Any().Should().BeFalse();
    }
    
    [Test, MoqAutoData]
    public async Task ThenAPopulatedResponseShouldBeReturnedWhenThereAreAgreements(
        long accountId,
        [Frozen] Mock<ILogger<AgreementOrchestrator>> logger,
        [Frozen] Mock<IMediator> mediator,
        [Frozen] Mock<IMapper> mapper,
        [NoAutoProperties] GetEmployerAgreementsByAccountIdResponse response)
    {
        response.EmployerAgreements = new List<EmployerAgreement>
        {
            new()
            {
                StatusId = EmployerAgreementStatus.Signed,
                Id = 1,
                Acknowledged = true
            },
            new()
            {
                StatusId = EmployerAgreementStatus.Signed,
                Id = 2,
                Acknowledged = false
            }
        };
        
        mediator.Setup(x => 
            x.Send(It.Is<GetEmployerAgreementsByAccountIdRequest>(c =>c.AccountId.Equals(accountId)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        
        var sut = new AgreementOrchestrator(mediator.Object, logger.Object, mapper.Object);
        var result = await sut.GetAgreements(accountId);

        result.Should().NotBeNull();
        result.Any().Should().BeTrue();

        result.Count().Should().Be(response.EmployerAgreements.Count);
        result.Should().BeEquivalentTo(response.EmployerAgreements, options => options.ExcludingMissingMembers());
    }
}