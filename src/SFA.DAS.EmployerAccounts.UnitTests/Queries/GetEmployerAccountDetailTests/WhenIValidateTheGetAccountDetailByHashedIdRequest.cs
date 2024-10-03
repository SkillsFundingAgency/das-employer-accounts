using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAccountDetail;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetEmployerAccountDetailTests
{
    public class WhenIValidateTheGetAccountDetailByHashedIdRequest
    {
        private GetEmployerAccountDetailByHashedIdValidator _validator;
       
        private const long ExpectedAccountId = 4567;  

        [SetUp]
        public void Arrange()
        {      
            _validator = new GetEmployerAccountDetailByHashedIdValidator();
        }

        [Test]
        public void ThenTheResultIsValidWhenAllFieldsArePopulatedAndTheUserIsPartOfTheAccount()
        {
            //Act
            var result = _validator.Validate(new GetEmployerAccountDetailByIdQuery { AccountId = ExpectedAccountId });

            //Assert
            result.IsValid().Should().BeTrue();
            result.IsUnauthorized.Should().BeFalse();
        }

        [Test]
        public void ThenTheUnauthorizedFlagIsSetWhenTheUserIsNotPartOfTheAccount()
        {
            //Act
            var result = _validator.Validate(new GetEmployerAccountDetailByIdQuery());

            //Assert
            result.IsUnauthorized.Should().BeFalse();
        }

        [Test]
        public void ThenTheDictionaryIsPopulatedWithValidationErrors()
        {
            //Act
            var result = _validator.Validate(new GetEmployerAccountDetailByIdQuery());

            //Assert
            result.IsUnauthorized.Should().BeFalse();
            result.ValidationDictionary
                .Should().ContainKey("AccountId")
                .WhoseValue.Should().Be("AccountId has not been supplied");
        }
    }
}
