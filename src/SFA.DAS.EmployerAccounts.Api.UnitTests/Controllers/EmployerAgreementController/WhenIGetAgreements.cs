using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using AutoMapper;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Api.Orchestrators;
using SFA.DAS.EmployerAccounts.Api.Types;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAgreementsByAccountId;
using SFA.DAS.Encoding;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Controllers.EmployerAgreementController;

public class WhenIGetAgreements
{
    [Test, MoqAutoData]
    public async Task Then_The_Handler_Called_And_Agreements_Returned(
        long accountId,
        [NoAutoProperties] GetEmployerAgreementsByAccountIdResponse response,
        [Frozen] Mock<IEncodingService> encodingService,
        [Frozen] Mock<IMediator> mediator)
    {
        response.EmployerAgreements = new List<EmployerAgreement>
        {
            new ()
            {
                Id = 1,
                Acknowledged = true,
                SignedDate = DateTime.Now,
            }
        };
        
        mediator.Setup(x => x.Send(It.Is<GetEmployerAgreementsByAccountIdRequest>(c => c.AccountId.Equals(accountId)),
            CancellationToken.None)).ReturnsAsync(response);

        var orchestrator = new AgreementOrchestrator(mediator.Object, Mock.Of<ILogger<AgreementOrchestrator>>(), Mock.Of<IMapper>());

        var controller = new Api.Controllers.EmployerAgreementController(orchestrator, encodingService.Object);

        var actual = await controller.GetAgreements(accountId) as OkObjectResult;
        var model = actual.Value as IEnumerable<EmployerAgreementView>;

        model.Should().NotBeNull();
        model.Should().BeEquivalentTo(response.EmployerAgreements, options => options.ExcludingMissingMembers());
    }
}