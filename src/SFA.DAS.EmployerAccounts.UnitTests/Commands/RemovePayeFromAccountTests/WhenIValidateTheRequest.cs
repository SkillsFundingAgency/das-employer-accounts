using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Models.AccountTeam;
using SFA.DAS.EmployerAccounts.Queries.RemovePayeFromAccount;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.RemovePayeFromAccountTests
{
    public class WhenIValidateTheRequest
    {
        private RemovePayeFromAccountCommandValidator _validator;
        private Mock<IMembershipRepository> _membershipRepository;


        [SetUp]
        public void Arrange()
        {
            _membershipRepository = new Mock<IMembershipRepository>();
            _membershipRepository.Setup(x => x.GetCaller(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new MembershipView { AccountId = 12345, Role = Role.Owner });

            _validator = new RemovePayeFromAccountCommandValidator(_membershipRepository.Object);
        }

        [Test]
        public async Task ThenItIsValidIfAllFieldsArePopulated()
        {
            //Act
            var result =
                await _validator.ValidateAsync(new RemovePayeFromAccountCommand("12345", "123RFD", "123edds", true,
                    "companyName"));


            //Assert
            Assert.That(result.IsValid(), Is.True);
        }

        [Test]
        public async Task ThenTheValidationDictionaryIsPopulatedWhenThereAreErrors()
        {
            //Act
            var result = await _validator.ValidateAsync(new RemovePayeFromAccountCommand(null, null, null, false, null));

            //Assert
            Assert.That(result.IsValid(), Is.False);
            Assert.That(result.ValidationDictionary, Is.Not.Empty);
            Assert.That(result.ValidationDictionary, Does.Contain(new KeyValuePair<string,string>("HashedAccountId", "HashedAccountId has not been supplied")));
            Assert.That(result.ValidationDictionary, Does.Contain(new KeyValuePair<string,string>("PayeRef","PayeRef has not been supplied")));
            Assert.That(result.ValidationDictionary, Does.Contain(new KeyValuePair<string,string>("UserId","UserId has not been supplied")));
            Assert.That(result.ValidationDictionary, Does.Contain(new KeyValuePair<string,string>("RemoveScheme", "Please confirm you wish to remove the scheme")));
        }

        [Test]
        public async Task ThenTheUserIsCheckedToSeeIfTheyAreAnOwnerAndUnauthroizedIsSetOnTheResult()
        {
            //Arrange
            _membershipRepository.Setup(x => x.GetCaller(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new MembershipView {AccountId = 12345, Role = Role.Transactor});

            //Act
            var result =
                await _validator.ValidateAsync(new RemovePayeFromAccountCommand("12345", "123RFD", "123edds", true,"companyName"));


            //Assert
            Assert.That(result.IsUnauthorized, Is.True);
        }

        [Test]
        public async Task ThenTheUserIsCheckedToSeeIfTheyArePartOfTheAccount()
        {
            //Arrange
            _membershipRepository.Setup(x => x.GetCaller(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(() => null);

            //Act
            var result = await _validator.ValidateAsync(new RemovePayeFromAccountCommand("12345", "123RFD", "123edds", true, "companyName"));


            //Assert
            Assert.That(result.IsUnauthorized, Is.True);
        }
    }
}
