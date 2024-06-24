using System.Collections.Generic;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAccountDetail;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetEmployerAccountDetailTests
{
    public class WhenIValidateTheGetAccountDetailByHashedIdRequest
    {
        private GetEmployerAccountDetailByHashedIdValidator _validator;
       
        private const string ExpectedHashedId = "4567";  

        [SetUp]
        public void Arrange()
        {      
            _validator = new GetEmployerAccountDetailByHashedIdValidator();
        }

        [Test]
        public void ThenTheResultIsValidWhenAllFieldsArePopulatedAndTheUserIsPartOfTheAccount()
        {
            //Act
            var result = _validator.Validate(new GetEmployerAccountDetailByHashedIdQuery { HashedAccountId = ExpectedHashedId });

            //Assert
            Assert.That(result.IsValid(), Is.True);
            Assert.That(result.IsUnauthorized, Is.False);
        }

        [Test]
        public void ThenTheUnauthorizedFlagIsSetWhenTheUserIsNotPartOfTheAccount()
        {
            //Act
            var result = _validator.Validate(new GetEmployerAccountDetailByHashedIdQuery());

            //Assert
            Assert.That(result.IsUnauthorized, Is.False);
        }

        [Test]
        public void ThenTheDictionaryIsPopulatedWithValidationErrors()
        {
            //Act
            var result = _validator.Validate(new GetEmployerAccountDetailByHashedIdQuery());

            //Assert
            Assert.That(result.IsValid(), Is.False);
            Assert.That(result.ValidationDictionary, Does.Contain(new KeyValuePair<string, string>("HashedAccountId", "HashedAccountId has not been supplied")));
        }
    }
}
