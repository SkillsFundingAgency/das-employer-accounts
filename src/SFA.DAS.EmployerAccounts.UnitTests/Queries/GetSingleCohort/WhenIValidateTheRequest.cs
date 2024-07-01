using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Queries.GetSingleCohort;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetSingleCohort
{
    public class WhenIValidateTheRequest
    {
        private GetSingleCohortRequestValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new GetSingleCohortRequestValidator();
        }

        [Test]
        public void ThenShouldReturnValidIfRequestIsValid()
        {
            //Act
            var result = _validator.Validate(new GetSingleCohortRequest { AccountId = 1231 });

            //Assert
            Assert.That(result.IsValid(), Is.True);
        }

        [Test]
        public void ThenShouldReturnInValidIfRequestIsNotValid()
        {
            //Act
            var result = _validator.Validate(new GetSingleCohortRequest { AccountId = 0 });

            //Assert
            Assert.That(result.IsValid(), Is.False);
        }
    }
}
