using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Queries.GetPayeSchemeByRef;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetPayeSchemeByRefTests
{
    public class WhenIGetAPayeScheme : QueryBaseTest<GetPayeSchemeByRefHandler, GetPayeSchemeByRefQuery, GetPayeSchemeByRefResponse>
    {
        private Mock<IPayeRepository> _payeRepository;
        public override GetPayeSchemeByRefQuery Query { get; set; }
        public override GetPayeSchemeByRefHandler RequestHandler { get; set; }
        public override Mock<IValidator<GetPayeSchemeByRefQuery>> RequestValidator { get; set; }

        private PayeSchemeView _expectedPayeScheme;
        private const long AccountId = 1667;
        private const string Ref = "ABC/123";

        [SetUp]
        public void Arrange()
        {
            SetUp();

            _expectedPayeScheme = new PayeSchemeView();

            _payeRepository = new Mock<IPayeRepository>();
            _payeRepository
                .Setup(x => x.GetPayeForAccountByRef(AccountId, Ref))
                .ReturnsAsync(_expectedPayeScheme);

            Query = new GetPayeSchemeByRefQuery
            {
                AccountId = AccountId,
                Ref = Ref
            };

            RequestHandler = new GetPayeSchemeByRefHandler(RequestValidator.Object, _payeRepository.Object);
        }

        [Test]
        public override async Task ThenIfTheMessageIsValidTheRepositoryIsCalled()
        {
            //Arrange
            RequestValidator.Setup(x => x.Validate(It.IsAny<GetPayeSchemeByRefQuery>())).Returns(new ValidationResult { ValidationDictionary = new Dictionary<string, string>() });

            //Act
            await RequestHandler.Handle(Query, CancellationToken.None);

            //Assert
            _payeRepository.Verify(x => x.GetPayeForAccountByRef(Query.AccountId, Query.Ref), Times.Once);
        }

        [Test]
        public override async Task ThenIfTheMessageIsValidTheValueIsReturnedInTheResponse()
        {
            //Arrange
            RequestValidator.Setup(x => x.Validate(It.IsAny<GetPayeSchemeByRefQuery>())).Returns(new ValidationResult { ValidationDictionary = new Dictionary<string, string>() });

            //Act
            var actual = await RequestHandler.Handle(Query, CancellationToken.None);

            //Assert
            actual.PayeScheme.Should().BeEquivalentTo(_expectedPayeScheme);
        }
    }
}
