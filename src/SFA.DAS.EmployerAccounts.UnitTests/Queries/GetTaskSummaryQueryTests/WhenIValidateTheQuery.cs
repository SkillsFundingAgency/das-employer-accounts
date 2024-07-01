using System.Collections.Generic;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Queries.GetTaskSummary;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetTaskSummaryQueryTests
{
    public class WhenIValidateTheQuery
    {
        private GetTaskSummaryQueryValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new GetTaskSummaryQueryValidator();
        }

        [Test]
        public void ThenFalseIsReturnedWhenTheQueryIsEmpty()
        {
            //Act
            var actual = _validator.Validate(new GetTaskSummaryQuery());

            //Assety
            Assert.That(actual.IsValid(), Is.False);
            Assert.That(actual.ValidationDictionary, Does.Contain(new KeyValuePair<string, string>("AccountId", "Account id must be supplied")));
        }

        [Test]
        public void ThenTrueIsReturnedWhenTheQueryIsPopulated()
        {
            //Act
            var actual = _validator.Validate(new GetTaskSummaryQuery { AccountId = 123 });

            //Assety
            Assert.That(actual.IsValid(), Is.True);
        }
    }
}