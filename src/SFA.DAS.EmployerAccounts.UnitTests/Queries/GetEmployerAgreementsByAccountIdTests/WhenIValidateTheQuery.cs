using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAgreementsByAccountId;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetEmployerAgreementsByAccountIdTests
{
    internal class WhenIValidateTheQuery
    {
        private GetEmployerAgreementsByAccountIdRequestValidator _validtor;

        [SetUp]
        public void Arrange()
        {
            _validtor = new GetEmployerAgreementsByAccountIdRequestValidator();
        }

        [Test]
        public void ThenValidationShouldFailIfThereIsNoAccountId()
        {
            //Arrange
            var request = new GetEmployerAgreementsByAccountIdRequest();

            //Act
            var result = _validtor.Validate(request);

            //Assert
            Assert.That(result.IsValid(), Is.False);
            Assert.That(result.ValidationDictionary.Count, Is.EqualTo(1));
        }
    }
}
