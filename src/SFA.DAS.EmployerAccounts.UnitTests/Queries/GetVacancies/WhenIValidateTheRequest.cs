using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Queries.GetVacancies;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetVacancies
{
    public class WhenIValidateTheRequest 
    {
        private GetVacanciesRequestValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new GetVacanciesRequestValidator();
        }

        [Test]
        public void ThenShouldReturnValidIfRequestIsValid()
        {
            //Act
            var result = _validator.Validate(new GetVacanciesRequest { AccountId = 1 });

            //Assert
            Assert.That(result.IsValid(), Is.True);
        }

        [Test]
        public void ThenShouldReturnInvalidIfAccountIdIsEmpty()
        {
            //Act
            var result = _validator.Validate(new GetVacanciesRequest { AccountId = 0});

            //Assert
            Assert.That(result.IsValid(), Is.False);
        }

    }
}
