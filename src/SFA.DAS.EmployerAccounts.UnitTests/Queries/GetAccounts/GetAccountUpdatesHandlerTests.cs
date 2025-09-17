using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Queries.GetAccounts;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetAccounts
{
    public class GetAccountUpdatesHandlerTests : QueryBaseTest<GetAccountsQueryHandler, GetAccountsQuery, GetAccountsResponse>
    {
        private Mock<IEmployerAccountRepository> _employerAccountRepository;
        public override GetAccountsQuery Query { get; set; }
        public override GetAccountsQueryHandler RequestHandler { get; set; }
        public override Mock<IValidator<GetAccountsQuery>> RequestValidator { get; set; }

        private string SinceDate = "0001-01-01 00:00:00.00000";
        private const int PageNumber = 1;
        private const int PageSize = 1000;

        [SetUp]
        public void Arrange()
        {
            
            SetUp();

            _employerAccountRepository = new Mock<IEmployerAccountRepository>();
            _employerAccountRepository
                .Setup(x => x.GetAllAccountsUpdates(SinceDate, PageNumber, PageSize))
                .ReturnsAsync(new GetAccountsResponse { Accounts = new Accounts<AccountUpdates> 
                {
                    AccountList = new List<AccountUpdates>
                    {
                        new AccountUpdates { AccountId = 1, AccountName = "Test1" },
                        new AccountUpdates { AccountId = 2, AccountName = "Test2" }
                    }
                } 
                });

            RequestHandler = new GetAccountsQueryHandler(_employerAccountRepository.Object, RequestValidator.Object);
            Query = new GetAccountsQuery
            {
                SinceDate = DateTime.MinValue,
                PageNumber = 1,
                PageSize = 1000
            };
        }

        [Test]
        public override async Task ThenIfTheMessageIsValidTheRepositoryIsCalled()
        {
            //Act
            await RequestHandler.Handle(Query, CancellationToken.None);

            //Assert
            _employerAccountRepository.Verify(x => x.GetAllAccountsUpdates(SinceDate, PageNumber, PageSize));
        }

        [Test]
        public override async Task ThenIfTheMessageIsValidTheValueIsReturnedInTheResponse()
        {
            //Act
            var result = await RequestHandler.Handle(Query, CancellationToken.None);

            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Accounts.AccountList, Is.Not.Null);
        }
    }

}