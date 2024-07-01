using System.Collections.Generic;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Queries.GetOrganisationsByAorn;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetOrganisationsByAornTests
{
    public class WhenIValidateTheQuery
    {
        private GetOrganisationsByAornValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new GetOrganisationsByAornValidator();
        }

        [Test]
        public void ThenTrueIsReturnedWhenAllFieldsArePopulated()
        {
            //Act
            var actual = _validator.Validate(new GetOrganisationsByAornRequest("aorn", "PayeRefTest"));

            //Assert
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.IsValid(), Is.True);
        }

        [Test]
        public void ThenFalseIsReturnedAndTheErrorDictionaryPopulatedWhenAornIsntPopulated()
        {
            //Act
            var actual = _validator.Validate(new GetOrganisationsByAornRequest("", "paye"));

            //Assert
            Assert.That(actual.IsValid(), Is.False);
            Assert.That(actual.ValidationDictionary, Does.Contain(new KeyValuePair<string,string>("Aorn","Aorn has not been supplied")));
        }

        [Test]
        public void ThenFalseIsReturnedAndTheErrorDictionaryPopulatedWhenPayeRefIsntPopulated()
        {
            //Act
            var actual = _validator.Validate(new GetOrganisationsByAornRequest("aorn", ""));

            //Assert
            Assert.That(actual.IsValid(), Is.False);
            Assert.That(actual.ValidationDictionary, Does.Contain(new KeyValuePair<string, string>("PayeRef", "PayeRef has not been supplied")));
        }
    }
}
