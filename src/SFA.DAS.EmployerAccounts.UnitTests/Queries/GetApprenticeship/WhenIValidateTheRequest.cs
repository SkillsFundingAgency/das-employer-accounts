using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Queries.GetApprenticeship;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetApprenticeship
{
    public class WhenIValidateTheRequest
    {
        private GetApprenticeshipsValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new GetApprenticeshipsValidator();
        }

 
        [Test]
        public void ThenShouldReturnValidIfRequestIsValid()
        {
            //Act
            var result = _validator.Validate(new GetApprenticeshipsRequest { AccountId = 4567, ExternalUserId = "user123" });

            //Assert
            Assert.That(result.IsValid(), Is.True);
        }

        [Test]
        public void ThenShouldReturnInvalidIfNoAccountIdIsProvided()
        {
            //Act
            var result = _validator.Validate(new GetApprenticeshipsRequest { ExternalUserId = "user123" });

            //Assert
            Assert.That(result.IsValid(), Is.False);
        }

        [Test]
        public void ThenShouldReturnInValidIfRequestIsNotValid()
        {
            //Act
            var result = _validator.Validate(new GetApprenticeshipsRequest { });

            //Assert
            Assert.That(result.IsValid(), Is.False);
        }
    }    
}
