using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Queries.GetReservations;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetReservations
{
    public class WhenIValidateTheRequest 
    {
        private GetReservationsRequestValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new GetReservationsRequestValidator();
        }

        [Test]
        public void ThenShouldReturnValidIfRequestIsValid()
        {
            //Act
            var result = _validator.Validate(new GetReservationsRequest { AccountId = 1876, ExternalUserId = "user123" });

            //Assert
            Assert.That(result.IsValid(), Is.True);
        }

        [Test]
        public void ThenShouldReturnInvalidIfNoAccountIdIsProvided()
        {
            //Act
            var result = _validator.Validate(new GetReservationsRequest { ExternalUserId = "user123" });

            //Assert
            Assert.That(result.IsValid(), Is.False);
        }

        [Test]
        public void ThenShouldReturnInvalidIfNoExternalUserIdIsProvided()
        {
            //Act
            var result = _validator.Validate(new GetReservationsRequest { AccountId = 1231 });

            //Assert
            Assert.That(result.IsValid(), Is.False);
        }

        [Test]
        public void ThenShouldReturnInvalidIfAccountIdIsEmpty()
        {
            //Act
            var result = _validator.Validate(new GetReservationsRequest { AccountId = 0, ExternalUserId = "user123" });

            //Assert
            Assert.That(result.IsValid(), Is.False);
        }

        [Test]
        public void ThenShouldReturnInvalidIfExternalUserIdIsEmpty()
        {
            //Act
            var result = _validator.Validate(new GetReservationsRequest { AccountId = 1231, ExternalUserId = string.Empty });

            //Assert
            Assert.That(result.IsValid(), Is.False);
        }
    }
}
