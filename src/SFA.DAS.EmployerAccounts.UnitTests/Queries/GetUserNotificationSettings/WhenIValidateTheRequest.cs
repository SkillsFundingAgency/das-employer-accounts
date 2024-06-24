using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Queries.GetUserNotificationSettings;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetUserNotificationSettings
{
    [TestFixture]
    public class WhenIValidateTheRequest
    {
        private GetUserNotificationSettingsQueryValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new GetUserNotificationSettingsQueryValidator();
        }

        [Test]
        public void ThenUserRefMustBeSupplied()
        {
            //Arrange
            var query = new GetUserNotificationSettingsQuery();

            //Act
            var result = _validator.Validate(query);

            //Assert
            Assert.That(result.IsValid(), Is.False);
            Assert.That(result.ValidationDictionary.ContainsKey(nameof(query.UserRef)), Is.True);
        }
    }
}