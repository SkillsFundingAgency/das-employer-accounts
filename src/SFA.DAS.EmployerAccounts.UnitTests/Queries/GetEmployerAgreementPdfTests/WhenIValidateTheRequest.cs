using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Models.AccountTeam;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAgreementPdf;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetEmployerAgreementPdfTests
{
    public class WhenIValidateTheRequest
    {
        private GetEmployerAgreementPdfValidator _validator;
        private Mock<IMembershipRepository> _membershipRepository;

        [SetUp]
        public void Arrange()
        {
            _membershipRepository = new Mock<IMembershipRepository>();
            _membershipRepository.Setup(x => x.GetCaller(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new MembershipView { Role = Role.Owner });

            _validator = new GetEmployerAgreementPdfValidator(_membershipRepository.Object);
        }

        [Test]
        public async Task ThenFalseIsReturnedAndTheErrorDictionaryPopulatedWhenTheFieldsArentPopulated()
        {
            //Act
            var actual = await _validator.ValidateAsync(new GetEmployerAgreementPdfRequest());

            //Assert
            Assert.That(actual.IsValid(), Is.False);
            Assert.That(actual.ValidationDictionary, Does.Contain(new KeyValuePair<string, string>("AccountId", "AccountId has not been supplied")));
            Assert.That(actual.ValidationDictionary, Does.Contain(new KeyValuePair<string, string>("LegalAgreementId", "LegalAgreementId has not been supplied")));
            Assert.That(actual.ValidationDictionary, Does.Contain(new KeyValuePair<string, string>("UserId", "UserId has not been supplied")));
        }

        [Test]
        public async Task ThenTrueIsReturnedWhenAllFieldsArePopulated()
        {
            //Act
            var actual = await _validator.ValidateAsync(new GetEmployerAgreementPdfRequest { AccountId = 1234, LegalAgreementId = 1231, UserId = "User" });

            //Assert
            Assert.That(actual.IsValid(), Is.True);
        }

        [Test]
        public async Task ThenIfIAmNotConnectedToTheAccountTheUnauthorizedFlagIsSet()
        {
            //Arrange
            _membershipRepository.Setup(x => x.GetCaller(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(() => null);

            //Act
            var actual = await _validator.ValidateAsync(new GetEmployerAgreementPdfRequest { AccountId = 1234, LegalAgreementId = 1231, UserId = "User" });

            //Act
            Assert.That(actual.IsUnauthorized, Is.True);
        }

        [TestCase(Role.None)]
        [TestCase(Role.Transactor)]
        [TestCase(Role.Viewer)]
        public async Task ThenIfIAmNotAnOwnerOfTheAccountThenTheUnauthorizedFlagIsSet(Role role)
        {
            //Arrange
            _membershipRepository.Setup(x => x.GetCaller(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new MembershipView { Role = role });

            //Act
            var actual = await _validator.ValidateAsync(new GetEmployerAgreementPdfRequest { AccountId = 1234, LegalAgreementId = 1231, UserId = "User" });

            //Act
            Assert.That(actual.IsUnauthorized, Is.True);
        }

    }
}
