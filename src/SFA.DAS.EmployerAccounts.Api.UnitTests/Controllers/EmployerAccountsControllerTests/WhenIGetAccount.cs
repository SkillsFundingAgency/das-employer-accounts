using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAccountDetail;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Controllers.EmployerAccountsControllerTests;

internal class WhenIGetAccount : EmployerAccountsControllerTests
{
    [Test]
    public async Task Then_Account_Is_Returned()
    {
        long accountId = 123;

        var accountResponse = new GetEmployerAccountDetailByIdResponse()
        {
            Account = new AccountDetail
            {
                AccountId = accountId,
                Name = "Test 1"
            }
        };

        Mediator
            .Setup(x => x.Send(
                It.Is<GetEmployerAccountDetailByIdQuery>(x => x.AccountId == accountId), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(accountResponse);

        var response = await Controller.GetAccount(accountId) as OkObjectResult;

        response.Should().NotBeNull();
        response.Value.Should().BeOfType<Types.AccountDetail>();
    }
}