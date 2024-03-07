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
            Assert.IsFalse(actual.IsValid());
            Assert.Contains(new KeyValuePair<string, string>("AccountId", "Account id must be supplied"), actual.ValidationDictionary);
        }

        [Test]
        public void ThenTrueIsReturnedWhenTheQueryIsPopulated()
        {
            //Act
            var actual = _validator.Validate(new GetTaskSummaryQuery { AccountId = 123 });

            //Assety
            Assert.IsTrue(actual.IsValid());
        }
    }
}