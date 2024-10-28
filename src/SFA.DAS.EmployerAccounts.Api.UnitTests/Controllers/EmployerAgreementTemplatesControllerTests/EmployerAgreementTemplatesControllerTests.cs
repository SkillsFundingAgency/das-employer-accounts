using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAgreementTemplates;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Controllers.EmployerAgreementTemplatesControllerTests;

public class EmployerAgreementTemplatesControllerTests
{
    [Test, MoqAutoData]
    public async Task Then_The_Handler_Called_And_Templates_Returned(
        [NoAutoProperties] GetEmployerAgreementTemplatesResponse response,
        [Frozen] Mock<IMediator> mediator,
        List<EmployerAgreementTemplate> templates)
    {
        response.EmployerAgreementTemplates = templates;

        mediator.Setup(m => m.Send(It.IsAny<GetEmployerAgreementTemplatesRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);

        var controller = new Api.Controllers.EmployerAgreementTemplatesController(mediator.Object);

        var actual = await controller.Get(CancellationToken.None) as OkObjectResult;
        var responseReturned = actual!.Value as GetEmployerAgreementTemplatesResponse;

        var model = responseReturned!.EmployerAgreementTemplates;

        model!.Should().NotBeNull();
        model!.Should()
            .BeEquivalentTo(response.EmployerAgreementTemplates);
    }
}