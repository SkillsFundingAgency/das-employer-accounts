using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Queries.GetUserByRef;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetUserByRefTests
{
    public class WhenIValidatedTheQuery
    {
        private GetUserByRefQueryValidator _queryValidator;

        [SetUp]
        public void Arrange()
        {
            _queryValidator = new GetUserByRefQueryValidator();
        }

        [Test]
        public void ThenIShouldPassIfIHaveAValudUserRef()
        {
            //Act
            var result = _queryValidator.Validate(new GetUserByRefQuery {UserRef = "123"});

            //Assert
            Assert.That(result.IsValid(), Is.True);
        }

        [Test]
        public void ThenIShouldFailIfIDoNotHaveAValudUserId()
        {
            //Act
            var result = _queryValidator.Validate(new GetUserByRefQuery());

            //Assert
            Assert.That(result.IsValid(), Is.False);
        }
    }
}
