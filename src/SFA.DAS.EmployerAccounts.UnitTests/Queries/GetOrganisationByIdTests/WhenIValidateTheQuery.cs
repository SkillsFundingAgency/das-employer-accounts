using System.Collections.Generic;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Queries.GetOrganisationById;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetOrganisationByIdTests
{
    public class WhenIValidateTheQuery
    {
        private GetOrganisationByIdValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new GetOrganisationByIdValidator();
        }

        [Test]
        public void ThenTrueIsReturnedWhenAllFieldsArePopulated()
        {
            //Act
            var actual = _validator.Validate(new GetOrganisationByIdRequest { Identifier = "Test", OrganisationType = OrganisationType.Other});

            //Assert
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.IsValid(), Is.True);
        }

        [Test]
        public void ThenFalseIsReturnedAndTheErrorDictionaryPopulatedWhenTheFieldsArentPopulated()
        {
            //Act
            var actual = _validator.Validate(new GetOrganisationByIdRequest { Identifier = "" });

            //Assert
            Assert.That(actual.IsValid(), Is.False);
            Assert.That(actual.ValidationDictionary, Does.Contain(new KeyValuePair<string,string>("Identifier", "Identifier has not been supplied")));
        }
    }
}
