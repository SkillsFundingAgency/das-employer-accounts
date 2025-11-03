using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Queries.GetAccountsSinceDate;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetAccountsSinceDate
{
    public class GetAccountsSinceDateQueryHandlerTests : QueryBaseTest<GetAccountsSinceDateQueryHandler, GetAccountsSinceDateQuery, GetAccountsSinceDateResponse>
    {
        private Mock<IEmployerAccountRepository> _employerAccountRepository;
        public override GetAccountsSinceDateQuery Query { get; set; }
        public override GetAccountsSinceDateQueryHandler RequestHandler { get; set; }
        public override Mock<IValidator<GetAccountsSinceDateQuery>> RequestValidator { get; set; }

        private DateTime SinceDate = DateTime.MinValue;
        private const int PageNumber = 1;
        private const int PageSize = 1000;

        [SetUp]
        public void Arrange()
        {
            
            SetUp();

            _employerAccountRepository = new Mock<IEmployerAccountRepository>();
            _employerAccountRepository
                .Setup(x => x.GetAccounts(SinceDate, PageNumber, PageSize))
                .ReturnsAsync(new Accounts<AccountNameSummary>
                {
                    AccountList = new List<AccountNameSummary>
                    {
                        new AccountNameSummary { Id = 1, Name = "Test1" },
                        new AccountNameSummary { Id = 2, Name = "Test2" }
                    }
                });

            RequestHandler = new GetAccountsSinceDateQueryHandler(_employerAccountRepository.Object, RequestValidator.Object);
            Query = new GetAccountsSinceDateQuery
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
            _employerAccountRepository.Verify(x => x.GetAccounts(SinceDate, PageNumber, PageSize));
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