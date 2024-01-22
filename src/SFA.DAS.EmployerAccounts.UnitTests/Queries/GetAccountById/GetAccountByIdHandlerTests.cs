using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Queries.GetAccountById;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetAccountById
{
    public class GetAccountByIdHandlerTests : QueryBaseTest<GetAccountByIdHandler, GetAccountByIdQuery, GetAccountByIdResponse>
    {
        private Mock<IEmployerAccountRepository> _employerAccountRepository;
        public override GetAccountByIdQuery Query { get; set; }
        public override GetAccountByIdHandler RequestHandler { get; set; }
        public override Mock<IValidator<GetAccountByIdQuery>> RequestValidator { get; set; }

        private const long ExpectedAccountId = 1876;

        [SetUp]
        public void Arrange()
        {
            SetUp();

            _employerAccountRepository = new Mock<IEmployerAccountRepository>();
            _employerAccountRepository.Setup(x => x.GetAccountById(ExpectedAccountId)).ReturnsAsync(new Account { Id = ExpectedAccountId });

            RequestHandler = new GetAccountByIdHandler(_employerAccountRepository.Object, RequestValidator.Object);
            Query = new GetAccountByIdQuery();
        }

        [Test]
        public override async Task ThenIfTheMessageIsValidTheRepositoryIsCalled()
        {
            //Act
            await RequestHandler.Handle(new GetAccountByIdQuery
            {
                AccountId = ExpectedAccountId
            }, CancellationToken.None);

            //Assert
            _employerAccountRepository.Verify(x => x.GetAccountById(ExpectedAccountId));
        }

        [Test]
        public override async Task ThenIfTheMessageIsValidTheValueIsReturnedInTheResponse()
        {
            //Act
            var result = await RequestHandler.Handle(new GetAccountByIdQuery
            {
                AccountId = ExpectedAccountId
            }, CancellationToken.None);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Account);
        }
    }
}