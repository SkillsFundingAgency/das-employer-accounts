using System.Collections.Generic;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.RemoveTeamMember;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.RemoveTeamMemberTests
{
    public class WhenIValidateTheCommand
    {
        private RemoveTeamMemberCommandValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new RemoveTeamMemberCommandValidator();
        }

        [Test]
        public void ThenTheErrorDictionaryIsPopulatedWhenThereAreFieldErrors()
        {
            //Act
            var actual = _validator.Validate(new RemoveTeamMemberCommand());

            //Assert
            Assert.That(actual.IsValid(), Is.False);
            Assert.That(actual.ValidationDictionary, Does.Contain(new KeyValuePair<string,string>("UserId", "No UserId supplied")));
            Assert.That(actual.ValidationDictionary, Does.Contain(new KeyValuePair<string,string>("HashedAccountId", "No HashedAccountId supplied")));
            Assert.That(actual.ValidationDictionary, Does.Contain(new KeyValuePair<string,string>("ExternalUserId", "No ExternalUserId supplied")));
            

        }
    }
}
