﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Models.AccountTeam;
using SFA.DAS.EmployerAccounts.Queries.GetAccountLegalEntityRemove;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetAccountEmployerAgreementRemove
{
    public class WhenIValidateTheQuery
    {
        private GetAccountLegalEntityRemoveValidator _validator;
        private Mock<IMembershipRepository> _membershipRepository;

        [SetUp]
        public void Arrange()
        {
            _membershipRepository = new Mock<IMembershipRepository>();
            _membershipRepository.Setup(x => x.GetCaller("ABC123", "XYZ987")).ReturnsAsync(new MembershipView { Role = Role.Owner });

            _validator = new GetAccountLegalEntityRemoveValidator(_membershipRepository.Object);
        }

        [Test]
        public async Task ThenFalseIsReturnedAndTheDictionaryIsPopulatedIfNoValuesAreSupplied()
        {
            //Act
            var actual = await _validator.ValidateAsync(new GetAccountLegalEntityRemoveRequest());

            //Assert
            Assert.That(actual.IsValid(), Is.False);
            Assert.That(actual.ValidationDictionary, Does.Contain(new KeyValuePair<string, string>("HashedAccountId", "HashedAccountId has not been supplied")));
            Assert.That(actual.ValidationDictionary, Does.Contain(new KeyValuePair<string, string>("UserId", "UserId has not been supplied")));
            Assert.That(actual.ValidationDictionary, Does.Contain(new KeyValuePair<string, string>("HashedAccountLegalEntityId", "HashedAccountLegalEntityId has not been supplied")));
            _membershipRepository.Verify(x => x.GetCaller(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task ThenTheUserIsCheckedToMakeSureThatTheyAreConnectedToTheAccount()
        {
            //Act
            _membershipRepository.Setup(x => x.GetCaller("ABC123", "XYZ987")).ReturnsAsync(() => null);
            var actual = await _validator.ValidateAsync(new GetAccountLegalEntityRemoveRequest { HashedAccountId = "ABC123", UserId = "XYZ987", HashedAccountLegalEntityId = "546TGF"});

            //Assert
            Assert.That(actual.IsUnauthorized, Is.True);
        }

        [Test]
        public async Task ThenTheUserIsCheckedToMakeSureThatTheyAreAnOwnerOnTheAccount()
        {
            //Arrange
            _membershipRepository.Setup(x => x.GetCaller("ABC123", "XYZ987")).ReturnsAsync(new MembershipView { Role = Role.Viewer });

            //Act
            var actual = await _validator.ValidateAsync(new GetAccountLegalEntityRemoveRequest { HashedAccountId = "ABC123", UserId = "XYZ987", HashedAccountLegalEntityId = "546TGF" });

            //Assert
            Assert.That(actual.IsUnauthorized, Is.True);
        }

        [Test]
        public async Task ThenIfAllFieldsArePopulatedAndTheUserIsAnOWnerOfTheAccountTrueIsReturned()
        {
            //Act
            var actual = await _validator.ValidateAsync(new GetAccountLegalEntityRemoveRequest { HashedAccountId = "ABC123", UserId = "XYZ987", HashedAccountLegalEntityId = "546TGF" });

            //Assert
            Assert.That(actual.IsValid(), Is.True);
            Assert.That(actual.IsUnauthorized, Is.False);
        }
    }
}
