using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Api.Types;
using SFA.DAS.EmployerAccounts.Queries.GetPagedEmployerAccounts;
using SFA.DAS.EmployerAccounts.TestCommon.Extensions;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Controllers.EmployerAccountsControllerTests
{
    [TestFixture]
    public class WhenIGetAccounts : EmployerAccountsControllerTests
    {
        [Test]
        public async Task ThenAccountsAreReturnedWithTheirAUriToGetAccountDetails()
        {
            // Arrange
            var pageNumber = 12;
            var pageSize = 50;
            var toDate = DateTime.Now.AddDays(-1).ToString("yyyyMMddHHmmss");

            var accountsResponse = new GetPagedEmployerAccountsResponse
            {
                AccountsCount = 2,
                Accounts = new List<Models.Account.Account>
                {
                    new() { HashedId = "ABC123", Id = 123, Name = "Test 1" },
                    new() { HashedId = "ABC999", Id = 987, Name = "Test 2" }
                }
            };
            Mediator
                .Setup(
                    x => x.Send(It.Is<GetPagedEmployerAccountsQuery>(q => q.PageNumber == pageNumber && q.PageSize == pageSize && q.ToDate == toDate),
                        It.IsAny<CancellationToken>())
                    )
                .ReturnsAsync(accountsResponse);


            UrlTestHelper
                  .Setup(
                  x => x.RouteUrl(
                      It.Is<UrlRouteContext>(c =>
                          c.RouteName == "GetAccountById" && c.Values.IsEquivalentTo(new { accountId = accountsResponse.Accounts[0].Id })))
                  ).Returns($"/api/accounts/{accountsResponse.Accounts[0].Id}");

            UrlTestHelper
                .Setup(x =>
                    x.RouteUrl(It.Is<UrlRouteContext>(
                        c => c.RouteName.Equals("GetAccountById") && c.Values.IsEquivalentTo(new { accountId = accountsResponse.Accounts[1].Id })))
                    ).Returns($"/api/accounts/{accountsResponse.Accounts[1].Id}");

            // Act
            var response = await Controller.GetAccounts(toDate, pageSize, pageNumber);

            // Assert
            response.Should().NotBeNull();
            var result = response.Should().BeAssignableTo<OkObjectResult>();
            var model = result.Subject.Value.Should().BeAssignableTo<PagedApiResponse<Account>>();

            model.Subject.Data.Should().NotBeNull();
            model.Subject.Page.Should().Be(pageNumber);
            model.Subject.Data.Should().HaveCount(accountsResponse.AccountsCount);

            foreach (var expectedAccount in accountsResponse.Accounts)
            {
                var returnedAccount = model.Subject.Data.SingleOrDefault(x => x.AccountId == expectedAccount.Id && x.HashedAccountId == expectedAccount.HashedId && x.AccountName == expectedAccount.Name);
                returnedAccount.Should().NotBeNull();
                returnedAccount?.Href.Should().Be($"/api/accounts/{returnedAccount.AccountId}");
            }
        }

        [Test]
        public async Task AndNoToDateIsProvidedThenAllAccountsAreReturned()
        {
            await Controller.GetAccounts();

            Mediator.Verify(x => x.Send(It.Is<GetPagedEmployerAccountsQuery>(q => q.ToDate == DateTime.MaxValue.ToString("yyyyMMddHHmmss")), It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task AndNoPageSizeIsProvidedThen1000AccountsAreReturned()
        {
            await Controller.GetAccounts(DateTime.Now.AddDays(-1).ToString("yyyyMMddHHmmss"));

            Mediator.Verify(x => x.Send(It.Is<GetPagedEmployerAccountsQuery>(q => q.PageSize == 1000), It.IsAny<CancellationToken>()));
        }

        [Test]
        public async Task AndNoPageNumberIsProvidedThenTheFirstPageOfAccountsAreReturned()
        {
            await Controller.GetAccounts(DateTime.Now.AddDays(-1).ToString("yyyyMMddHHmmss"));

            Mediator.Verify(x => x.Send(It.Is<GetPagedEmployerAccountsQuery>(q => q.PageNumber == 1), It.IsAny<CancellationToken>()));
        }
    }
}
