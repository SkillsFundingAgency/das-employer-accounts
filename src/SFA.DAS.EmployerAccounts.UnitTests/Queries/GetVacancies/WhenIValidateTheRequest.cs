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
            var result = _validator.Validate(new GetVacanciesRequest { HashedAccountId = "123ABC", ExternalUserId = "user123" });

            //Assert
            Assert.That(result.IsValid(), Is.True);
        }

        [Test]
        public void ThenShouldReturnInvalidIfNoAccountIdIsProvided()
        {
            //Act
            var result = _validator.Validate(new GetVacanciesRequest { ExternalUserId = "user123" });

            //Assert
            Assert.That(result.IsValid(), Is.False);
        }

        [Test]
        public void ThenShouldReturnInvalidIfNoExternalUserIdIsProvided()
        {
            //Act
            var result = _validator.Validate(new GetVacanciesRequest { HashedAccountId = "123ABC" });

            //Assert
            Assert.That(result.IsValid(), Is.False);
        }

        [Test]
        public void ThenShouldReturnInvalidIfAccountIdIsEmpty()
        {
            //Act
            var result = _validator.Validate(new GetVacanciesRequest { HashedAccountId = string.Empty, ExternalUserId = "user123" });

            //Assert
            Assert.That(result.IsValid(), Is.False);
        }

        [Test]
        public void ThenShouldReturnInvalidIfExternalUserIdIsEmpty()
        {
            //Act
            var result = _validator.Validate(new GetVacanciesRequest { HashedAccountId = "123ABC", ExternalUserId = string.Empty });

            //Assert
            Assert.That(result.IsValid(), Is.False);
        }
    }
}
