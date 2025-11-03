using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Queries.GetAccountsSinceDate;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetAccountsSinceDate
{
    public class GetAccountsSinceDateQueryValidatorTests
    {
        [Test]
        [MoqInlineAutoData(0)]
        [MoqInlineAutoData(-1)]
        [MoqInlineAutoData(-999)]
        public async Task ThenTheRequestIsNotValidWhenPageNumberIsLessThanOrEqualToZero(
           int pageNumber,
           GetAccountsSinceDateQuery query,
           GetAccountsSinceDateQueryValidator validator)
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
           GetAccountsSinceDateQuery query,
           GetAccountsSinceDateQueryValidator validator)
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
