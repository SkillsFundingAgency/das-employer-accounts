using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Queries.GetAccountLegalEntities;
using SFA.DAS.EmployerAccounts.Queries.GetAccountLegalEntitiesByHashedAccountId;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetAccountLegalEntitiesByHashedAccountId
{
    public class WhenValidatingAccountLegalEntitiesByHashedAccountId
    {
        private GetAccountLegalEntitiesByHashedAccountIdValidator _validator;

        [SetUp]
        public void Arange()
        {
            _validator = new GetAccountLegalEntitiesByHashedAccountIdValidator();
        }

        [Test]
        public void ThenFalseIsReturnedIfTheFieldsArentPopulated()
        {
            //Act
            var result = _validator.Validate(new GetAccountLegalEntitiesByHashedAccountIdRequest());

            //Assert
            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.ContainsKey("AccountId");
            result.ValidationDictionary.ContainsValue("AccountId has not been supplied");
        }

        [Test]
        public void ThenTrueIsReturnedIfTheFieldsArePopulated()
        {
            //Act
            var result = _validator.Validate(new GetAccountLegalEntitiesByHashedAccountIdRequest { AccountId = 12345});

            //Assert
            result.IsValid().Should().BeTrue();
        }
    }
}