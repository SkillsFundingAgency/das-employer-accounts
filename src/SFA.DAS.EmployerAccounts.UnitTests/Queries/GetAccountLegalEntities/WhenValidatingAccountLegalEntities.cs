using System;
using System.Collections.Generic;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Queries.GetAccountLegalEntities;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetAccountLegalEntities
{
    public class WhenValidatingAccountLegalEntities
    {
        private GetAccountLegalEntitiesValidator _validator;

        [SetUp]
        public void Arange()
        {
            _validator = new GetAccountLegalEntitiesValidator();
        }

        [Test]
        public void ThenFalseIsReturnedIfTheFieldsArentPopulated()
        {
            //Act
            var result = _validator.Validate(new GetAccountLegalEntitiesRequest());

            //Assert
            Assert.That(result.IsValid(), Is.False);
            Assert.That(result.ValidationDictionary, Does.Contain(new KeyValuePair<string,string>("HashedLegalEntityId", "HashedLegalEntityId has not been supplied")));
            Assert.That(result.ValidationDictionary, Does.Contain(new KeyValuePair<string,string>("UserId","User Id has not been supplied")));
        }

        [Test]
        public void ThenFalseIsReturnedIfTheUserIdIsNotAGuid()
        {
            //Act
            var result = _validator.Validate(new GetAccountLegalEntitiesRequest {HashedLegalEntityId="12345",UserId = "12345"});

            //Assert
            Assert.That(result.IsValid(), Is.False);
            Assert.That(result.ValidationDictionary, Does.Contain(new KeyValuePair<string, string>("UserId", "User Id has not been supplied in the correct format")));
        }

        [Test]
        public void ThenTrueIsReturnedIfTheFieldsArePopulated()
        {
            //Act
            var result = _validator.Validate(new GetAccountLegalEntitiesRequest { HashedLegalEntityId = "12345", UserId = Guid.NewGuid().ToString() });

            //Assert
            Assert.That(result.IsValid(), Is.True);
        }
    }
}