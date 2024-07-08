using System.Collections.Generic;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Queries.GetOrganisations;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetOrganisationTests
{
    public class WhenIValidateTheQuery
    {
        private GetOrganisationsValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new GetOrganisationsValidator();
        }

        [Test]
        public void ThenTrueIsReturnedWhenAllFieldsArePopulated()
        {
            //Act
            var actual = _validator.Validate(new GetOrganisationsRequest {SearchTerm = "Test Company"});

            //Assert
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.IsValid(), Is.True);
        }

        [Test]
        public void ThenFalseIsReturnedAndTheErrorDictionaryPopulatedWhenTheFieldsArentPopulated()
        {
            //Act
            var actual = _validator.Validate(new GetOrganisationsRequest { SearchTerm = "" });

            //Assert
            Assert.That(actual.IsValid(), Is.False);
            Assert.That(actual.ValidationDictionary, Does.Contain(new KeyValuePair<string,string>("SearchTerm","SearchTerm has not been supplied")));
        }
    }
}
