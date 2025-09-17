using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Api.Types;
using SFA.DAS.EmployerAccounts.Exceptions;
using SFA.DAS.EmployerAccounts.Queries.GetAccounts;
using System.Net;
using System.Threading.Tasks;
using SFA.DAS.EmployerAccounts.Validation;


namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Controllers.EmployerAccountsControllerTests
{
    [TestFixture]
    public class WhenIGetAccountUpdates : EmployerAccountsControllerTests
    {
        [Test]
        public async Task Then_I_Get_All_Accounts()
        {
            var pageSize = 1000;
            var pageNumber = 1;
            var toDate = System.DateTime.MinValue;

            var accountsResponse = new GetAccountsResponse
            {
                Accounts = new Models.Account.Accounts<AccountUpdates>
                {
                    AccountList = new System.Collections.Generic.List<AccountUpdates>
                    {
                        new AccountUpdates { AccountId = 1, AccountName = "Test Account 1" },
                        new AccountUpdates { AccountId = 2, AccountName = "Test Account 2" }
                    },
                    AccountsCount = 2
                }
            };

            MediatorMock
                .Setup(x => x.Send(It.Is<GetAccountsQuery>(q => q.PageNumber == pageNumber && q.PageSize == pageSize && q.SinceDate == toDate), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(accountsResponse);

            var response = await Controller.GetAccountUpdates(toDate, pageNumber, pageSize);
            var result = response.Should().BeAssignableTo<OkObjectResult>();
            var model = result.Subject.Value.Should().BeAssignableTo<PagedApiResponse<AccountUpdates>>();

            model.Subject.Data.Count.Should().Be(2);
            model.Subject.Page.Should().Be(pageNumber);
            model.Subject.TotalPages.Should().Be(1);

            foreach (var account in accountsResponse.Accounts.AccountList)
            {
                var accountInResponse = model.Subject.Data.Find(a => a.AccountId == account.AccountId);
                accountInResponse.Should().NotBeNull();
                accountInResponse.AccountName.Should().Be(account.AccountName);
            }
        }

        [Test]
        public async Task Then_If_No_Accounts_Then_An_Empty_List_Is_Returned()
        {
            var pageSize = 1000;
            var pageNumber = 1;
            var toDate = System.DateTime.MinValue;
            var accountsResponse = new GetAccountsResponse
            {
                Accounts = new Models.Account.Accounts<AccountUpdates>
                {
                    AccountList = new System.Collections.Generic.List<AccountUpdates>(),
                    AccountsCount = 0
                }
            };
            MediatorMock
                .Setup(x => x.Send(It.Is<GetAccountsQuery>(q => q.PageNumber == pageNumber && q.PageSize == pageSize && q.SinceDate == toDate), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(accountsResponse);
            var response = await Controller.GetAccountUpdates(toDate, pageNumber, pageSize);
            var result = response.Should().BeAssignableTo<OkObjectResult>();
            var model = result.Subject.Value.Should().BeAssignableTo<PagedApiResponse<AccountUpdates>>();
           
            model.Subject.Data.Count.Should().Be(0);
            model.Subject.Page.Should().Be(pageNumber);
            model.Subject.TotalPages.Should().Be(1);
        }

        [Test]
        public async Task Then_If_Accounts_Less_Than_PageSize_Then_Only_One_Page_Returned()
        {
            var pageSize = 2;
            var pageNumber = 1;
            var toDate = System.DateTime.MinValue;
            var accountsResponse = new GetAccountsResponse
            {
                Accounts = new Models.Account.Accounts<AccountUpdates>
                {
                    AccountList = new System.Collections.Generic.List<AccountUpdates>
                    {
                        new AccountUpdates { AccountId = 1, AccountName = "Test Account 1" }
                    },
                    AccountsCount = 1
                }
            };
            MediatorMock
                .Setup(x => x.Send(It.Is<GetAccountsQuery>(q => q.PageNumber == pageNumber && q.PageSize == pageSize && q.SinceDate == toDate), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(accountsResponse);
            var response = await Controller.GetAccountUpdates(toDate, pageNumber, pageSize);
            var result = response.Should().BeAssignableTo<OkObjectResult>();
            var model = result.Subject.Value.Should().BeAssignableTo<PagedApiResponse<AccountUpdates>>();
            model.Subject.Data.Count.Should().Be(1);
            model.Subject.Page.Should().Be(pageNumber);
            model.Subject.TotalPages.Should().Be(1);
        }

        [Test]
        public async Task Then_If_PageSize_Is_Zero_Then_InvalidRequestException_Is_Thrown()
        {
            var validationResult = new ValidationResult();
            validationResult.AddError("PageSize", "Page size must be greater than zero when provided");

            var pageSize = 0;
            var pageNumber = 1;
            var toDate = System.DateTime.MinValue;

            MediatorMock
                .Setup(x => x.Send(It.Is<GetAccountsQuery>(q => q.PageNumber == pageNumber && q.PageSize == pageSize && q.SinceDate == toDate), It.IsAny<System.Threading.CancellationToken>()))
                .Throws(new InvalidRequestException(validationResult.ValidationDictionary));

            var response = await Controller.GetAccountUpdates(toDate, pageNumber, pageSize);
            var result = response.Should().BeAssignableTo<StatusCodeResult>();
            result.Subject.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task Then_If_PageNumber_Is_Zero_Then_InvalidRequestException_Is_Thrown()
        {
            var validationResult = new ValidationResult();
            validationResult.AddError("PageNumber", "Page number must be greater than zero when provided");
            var pageSize = 1000;
            var pageNumber = 0;
            var toDate = System.DateTime.MinValue;
            MediatorMock
                .Setup(x => x.Send(It.Is<GetAccountsQuery>(q => q.PageNumber == pageNumber && q.PageSize == pageSize && q.SinceDate == toDate), It.IsAny<System.Threading.CancellationToken>()))
                .Throws(new InvalidRequestException(validationResult.ValidationDictionary));
         
            var response = await Controller.GetAccountUpdates(toDate, pageNumber, pageSize);
            var result = response.Should().BeAssignableTo<StatusCodeResult>();
            result.Subject.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task Then_If_Mediator_Throws_Exception_Then_InternalServerError_Is_Returned()
        {
            var pageSize = 1000;
            var pageNumber = 1;
            var toDate = System.DateTime.MinValue;
            MediatorMock
                .Setup(x => x.Send(It.Is<GetAccountsQuery>(q => q.PageNumber == pageNumber && q.PageSize == pageSize && q.SinceDate == toDate), It.IsAny<System.Threading.CancellationToken>()))
                .Throws(new System.Exception());
            var response = await Controller.GetAccountUpdates(toDate, pageNumber, pageSize);
            var result = response.Should().BeAssignableTo<StatusCodeResult>();
            result.Subject.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        }
    }
}
