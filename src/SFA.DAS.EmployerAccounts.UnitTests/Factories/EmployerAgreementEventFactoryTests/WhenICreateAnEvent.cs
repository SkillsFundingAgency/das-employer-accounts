using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Factories;

namespace SFA.DAS.EmployerAccounts.UnitTests.Factories.EmployerAgreementEventFactoryTests
{
    internal class WhenICreateAnEvent
    {
        private EmployerAgreementEventFactory _factory;
        
        [SetUp]
        public void Arrange()
        {

            _factory = new EmployerAgreementEventFactory();
        }

        [Test]
        public void ThenIShouldGetTheCorrectResourceUrl()
        {
            //Arrange
            const string hashedAccountId = "HJKH23423";
            const string hashedLegalEntityId = "FDF654";
            const string hashedAgreementId = "3GF3KH";
            
            var expectedApiUrlPostfix =
                $"api/accounts/{hashedAccountId}/legalEntities/{hashedLegalEntityId}/agreements/{hashedAgreementId}";

            //Act
            var @event = _factory.CreateSignedEvent(hashedAccountId, hashedLegalEntityId, hashedAgreementId);

            //Assert
            Assert.That(@event.ResourceUrl, Is.EqualTo(expectedApiUrlPostfix));
        }

        [Test]
        public void ThenIGetTheCorrectParamtersSetWhenRemovingTheAgreement()
        {
            //Arrange
            const string hashedAgreementId = "3GF3KH";

            //Act
            var actual = _factory.RemoveAgreementEvent(hashedAgreementId);

            //Assert
            Assert.That(actual.HashedAgreementId, Is.EqualTo(hashedAgreementId));

        }
    }
}
