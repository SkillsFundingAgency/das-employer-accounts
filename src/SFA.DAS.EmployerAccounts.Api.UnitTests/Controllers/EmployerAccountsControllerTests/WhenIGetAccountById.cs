using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Queries.GetAccountById;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Controllers.EmployerAccountsControllerTests
{
    internal class WhenIGetAccountById : EmployerAccountsControllerTests
    {
        [Test]
        public async Task Then_Account_Is_Returned()
        {
            long accountId = 123;

            var accountResponse = new GetAccountByIdResponse
            {
                Account = new Models.Account.Account()
                {
                    Id = accountId,
                    Name = "Test 1"
                }
            };

            Mediator.Setup(x => x.Send(It.IsAny<GetAccountByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(accountResponse);

            var response = await Controller.GetAccountById(accountId) as OkObjectResult;

            response.Should().NotBeNull();
            response.Value.Should().BeOfType<Types.AccountDetail>();
        }
    }
}