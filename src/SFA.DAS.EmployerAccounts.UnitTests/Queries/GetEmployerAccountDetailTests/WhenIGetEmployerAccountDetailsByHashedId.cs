using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAccountDetail;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetEmployerAccountDetailTests
{
    public class WhenIGetEmployerAccountDetailsByHashedId : QueryBaseTest<GetEmployerAccountDetailByHashedIdHandler, GetEmployerAccountDetailByIdQuery, GetEmployerAccountDetailByIdResponse>
    {
        private Mock<IEmployerAccountRepository> _employerAccountRepository;
        public override GetEmployerAccountDetailByIdQuery Query { get; set; }
        public override GetEmployerAccountDetailByHashedIdHandler RequestHandler { get; set; }
        public override Mock<IValidator<GetEmployerAccountDetailByIdQuery>> RequestValidator { get; set; }

        private const long ExpectedAccountId = 889299;
        private const string ExpectedHashedAccountId = "MNBGBD";
       
        [SetUp]
        public void Arrange()
        {
            SetUp();

            _employerAccountRepository = new Mock<IEmployerAccountRepository>();
            _employerAccountRepository
                .Setup(x => x.GetAccountDetailById(ExpectedAccountId))
                .ReturnsAsync(new AccountDetail { HashedId = ExpectedHashedAccountId });

            RequestHandler = new GetEmployerAccountDetailByHashedIdHandler(RequestValidator.Object, _employerAccountRepository.Object);
            Query = new GetEmployerAccountDetailByIdQuery();
        }

        [Test]
        public override async Task ThenIfTheMessageIsValidTheRepositoryIsCalled()
        {
            //Act
            await RequestHandler.Handle(new GetEmployerAccountDetailByIdQuery
            {
                AccountId = ExpectedAccountId
            }, CancellationToken.None);

            //Assert
            _employerAccountRepository.Verify(x => x.GetAccountDetailById(ExpectedAccountId));
        }

        [Test]
        public override async Task ThenIfTheMessageIsValidTheValueIsReturnedInTheResponse()
        {
            //Act
            var result = await RequestHandler.Handle(new GetEmployerAccountDetailByIdQuery
            {
                AccountId = ExpectedAccountId
            }, CancellationToken.None);

            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Account, Is.Not.Null);
        }
    }
}
