using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAgreementById;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetEmployerAgreementByIdTests
{
    internal class WhenIValidateTheQuery
    {
        private GetEmployerAgreementByIdRequestValidator _validtor;

        [SetUp]
        public void Arrange()
        {
            _validtor = new GetEmployerAgreementByIdRequestValidator();
        }

        [Test]
        public void ThenValidationShouldIfThereIsNoHashAgreementId()
        {
            //Arrange
            var request = new GetEmployerAgreementByIdRequest();

            //Act
            var result = _validtor.Validate(request);

            //Assert
            Assert.That(result.IsValid(), Is.False);
            Assert.That(result.ValidationDictionary.Count, Is.EqualTo(1));
        }
    }
}
