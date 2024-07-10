using System.Collections.Generic;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Queries.GetAccountById;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetAccountById
{
    public class WhenIValidateGetAccountByIdRequest
    {
        [Test]
        [MoqInlineAutoData(0)]
        [MoqInlineAutoData(-1)]
        [MoqInlineAutoData(-999)]
        public void ThenTheRequestIsNotValidIfAccountIdInvalid(
           long accountId,
           GetAccountByIdQuery query,
           GetAccountByIdValidator validator)
        {
            //Arrange
            query.AccountId = accountId;

            //Act
            var actual = validator.Validate(query);

            //Assert
            Assert.That(actual.IsValid(), Is.False);
            Assert.That(actual.ValidationDictionary, Does.Contain(new KeyValuePair<string, string>("AccountId", "Account ID has not been supplied")));
        }
    }
}
