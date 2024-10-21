using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.AcknowledgeTrainingProviderTask;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Controllers.EmployerAccountsControllerTests;

public class WhenIAcknowledgeTrainingProviderTask : EmployerAccountsControllerTests
{
    [Test, MoqAutoData]
    public async Task ThenOkResultIsReturned(long accountId)
    {
        var response = await Controller.AcknowledgeTrainingProviderTask(new AcknowledgeTrainingProviderTaskCommand(accountId));

        response.Should().NotBeNull();
        response.Should().BeOfType<OkResult>();
    }
}