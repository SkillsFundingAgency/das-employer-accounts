using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Queries.GetAccountById;
using SFA.DAS.EmployerAccounts.Queries.GetAccounts;
using SFA.DAS.Testing.AutoFixture;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetAccounts
{
    public class GetAccountsValidatorTest
    {
        [Test]
        [MoqInlineAutoData(0)]
        [MoqInlineAutoData(-1)]
        [MoqInlineAutoData(-999)]
        public async Task ThenTheRequestIsNotValidWhenPageNumberIsLessThanOrEqualToZero(
           int pageNumber,
           GetAccountsQuery query,
           GetAccountsQueryValidator validator)
        {
            //Arrange
            query.PageNumber = pageNumber;
            query.PageSize = 10;
            query.SinceDate = System.DateTime.MinValue;

            //Act
            var actual = await validator.ValidateAsync(query);

            //Assert
            Assert.That(actual.IsValid(), Is.False);
            Assert.That(actual.ValidationDictionary, Does.Contain(new KeyValuePair<string, string>("PageNumber", "Page number must be greater than zero when provided")));
        }

        [Test]
        [MoqInlineAutoData(0)]
        [MoqInlineAutoData(-1)]
        [MoqInlineAutoData(-999)]
        public async Task ThenTheRequestIsNotValidWhenPageSizeIsLessThanOrEqualToZero(
           int pageSize,
           GetAccountsQuery query,
           GetAccountsQueryValidator validator)
        {
            //Arrange
            query.PageNumber = 1;
            query.PageSize = pageSize;
            query.SinceDate = System.DateTime.MinValue;

            //Act
            var actual = await validator.ValidateAsync(query);

            //Assert
            Assert.That(actual.IsValid(), Is.False);
            Assert.That(actual.ValidationDictionary, Does.Contain(new KeyValuePair<string, string>("PageSize", "Page size must be greater than zero when provided")));
        }
    }
}
