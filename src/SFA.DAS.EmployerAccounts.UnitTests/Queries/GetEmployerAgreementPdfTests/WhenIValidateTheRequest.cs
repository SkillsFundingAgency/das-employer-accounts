﻿using System.Collections.Generic;
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
            Assert.IsFalse(actual.IsValid());
            Assert.Contains(new KeyValuePair<string, string>("AccountId", "AccountId has not been supplied"), actual.ValidationDictionary);
            Assert.Contains(new KeyValuePair<string, string>("LegalAgreementId", "LegalAgreementId has not been supplied"), actual.ValidationDictionary);
            Assert.Contains(new KeyValuePair<string, string>("UserId", "UserId has not been supplied"), actual.ValidationDictionary);
        }

        [Test]
        public async Task ThenTrueIsReturnedWhenAllFieldsArePopulated()
        {
            //Act
            var actual = await _validator.ValidateAsync(new GetEmployerAgreementPdfRequest { AccountId = 1234, LegalAgreementId = 1231, UserId = "User" });

            //Assert
            Assert.IsTrue(actual.IsValid());
        }

        [Test]
        public async Task ThenIfIAmNotConnectedToTheAccountTheUnauthorizedFlagIsSet()
        {
            //Arrange
            _membershipRepository.Setup(x => x.GetCaller(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(() => null);

            //Act
            var actual = await _validator.ValidateAsync(new GetEmployerAgreementPdfRequest { AccountId = 1234, LegalAgreementId = 1231, UserId = "User" });

            //Act
            Assert.IsTrue(actual.IsUnauthorized);
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
            Assert.IsTrue(actual.IsUnauthorized);
        }

    }
}
