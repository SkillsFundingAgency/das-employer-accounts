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
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAccountDetail;
using SFA.DAS.EmployerAccounts.TestCommon.Extensions;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Controllers.EmployerAccountsControllerTests
{
    public class WhenIGetAnAccountWithLegalEntities : EmployerAccountsControllerTests
    {
        [Test]
        public async Task ThenTheAccountIsReturnedAndTheUriIsCorrect()
        {
            // Arrange
            var accountId = 11;
            var hashedAccountId = "ABC123";

            var accountsResponse = new GetEmployerAccountDetailByIdResponse
            {
                Account = new Models.Account.AccountDetail()
                {
                    AccountId = accountId,
                    HashedId = hashedAccountId,
                    Name = "Test 1",
                    LegalEntities = new List<long> { 234, 123 }
                }
            };

            Mediator.Setup(x => x.Send(
                    It.Is<GetEmployerAccountDetailByIdQuery>(x => x.AccountId == accountId), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(accountsResponse);
            
             UrlTestHelper.Setup(x => x.RouteUrl(
                 It.Is<UrlRouteContext>(c =>
                     c.RouteName == "GetLegalEntity" && c.Values.IsEquivalentTo(new {  accountId, legalEntityId = accountsResponse.Account.LegalEntities[0].ToString() })))
             )
                 .Returns($"/api/accounts/{accountId}/legalEntity/{accountsResponse.Account.LegalEntities[0]}");
             
             UrlTestHelper.Setup(x => x.RouteUrl(
                     It.Is<UrlRouteContext>(c =>
                         c.RouteName == "GetLegalEntity" && c.Values.IsEquivalentTo(new { accountId, legalEntityId = accountsResponse.Account.LegalEntities[1].ToString() })))
                 )
                 .Returns($"/api/accounts/{accountId}/legalEntity/{accountsResponse.Account.LegalEntities[1]}");
             
            // Act
            var response = await Controller.GetAccount(accountId);
            
            // Assert
            var result = response.Should().BeAssignableTo<OkObjectResult>();
            var model = result.Subject.Value as AccountDetail;

            model.Should().NotBeNull();
            model.AccountId.Should().Be(accountId);
            model.DasAccountName.Should().Be("Test 1");
            model.HashedAccountId.Should().Be(hashedAccountId);

            foreach (var legalEntity in accountsResponse.Account.LegalEntities)
            {
                var matchedScheme = model.LegalEntities.Single(x => x.Id == legalEntity.ToString());
                matchedScheme?.Href.Should().Be($"/api/accounts/{accountId}/legalEntity/{legalEntity}");
            }
        }
    }
}
