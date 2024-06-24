using System.Collections.Generic;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Queries.GetPayeSchemeInUse;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetPayeSchemeInUseTests
{
    public class WhenIValidateTheQuery
    {
        private GetPayeSchemeInUseValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new GetPayeSchemeInUseValidator();
        }

        [Test]
        public void ThenFalseIsReturnedWhenTheQueryIsEmpty()
        {
            //Act
            var actual = _validator.Validate(new GetPayeSchemeInUseQuery());

            //Assety
            Assert.That(actual.IsValid(), Is.False);
            Assert.That(actual.ValidationDictionary, Does.Contain(new KeyValuePair<string,string>("Empref", "Empref has not been supplied")));
        }

        [Test]
        public void ThenTrueIsReturnedWhenTheQueryIsPopulated()
        {
            //Act
            var actual = _validator.Validate(new GetPayeSchemeInUseQuery {Empref = "AFV123"});

            //Assety
            Assert.That(actual.IsValid(), Is.True);
        }
    }
}