﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.AddPayeToAccount;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Models.AccountTeam;
using SFA.DAS.EmployerAccounts.UnitTests.ObjectMothers;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.AddPayeToAccountTests
{
    public class WhenIValidateTheCommand
    {
        private AddPayeToAccountCommandValidator _validator;
        private Mock<IMembershipRepository> _membershiprepository;
        private const string ExpectedOwnerUserId = "123ABC";
        private const string ExpectedNonOwnerUserId = "543FDC";

        [SetUp]
        public void Arrange()
        {
            _membershiprepository = new Mock<IMembershipRepository>();
            _membershiprepository.Setup(x => x.GetCaller(It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(() => null);
            _membershiprepository.Setup(x => x.GetCaller(It.IsAny<string>(), ExpectedOwnerUserId)).ReturnsAsync(new MembershipView {Role = Role.Owner});
            _membershiprepository.Setup(x => x.GetCaller(It.IsAny<string>(), ExpectedNonOwnerUserId)).ReturnsAsync(new MembershipView {Role = Role.Viewer});

            _validator = new AddPayeToAccountCommandValidator(_membershiprepository.Object);
        }

        [Test]
        public async Task ThenTheCommandIsInvalidIfTheFieldsArentPopulated()
        {
            //Act
            var actual = await _validator.ValidateAsync(new AddPayeToAccountCommand());

            //Assert
            Assert.That(actual.IsValid(), Is.False);
            _membershiprepository.Verify(x => x.Get(It.IsAny<long>(), It.IsAny<long>()), Times.Never);
        }

        [Test]
        public async Task ThenTheMembershipRepositoryIsCheckedToMakeSureTheUserIsAnAccountOwner()
        {
            //Arrange
            var command = AddPayeToNewLegalEntityCommandObjectMother.Create();

            //Act
            await _validator.ValidateAsync(command);

            //Assert
            _membershiprepository.Verify(x=>x.GetCaller(command.HashedAccountId, command.ExternalUserId), Times.Once);
        }

        [Test]
        public async Task ThenTheCommandIsInvalidIfTheUserIsNotPartOfTheAccount()
        {
            //Arrange
            var command = AddPayeToNewLegalEntityCommandObjectMother.Create("ABC");

            //Act
            var actual = await _validator.ValidateAsync(command);

            //Assert
            _membershiprepository.Verify(x => x.GetCaller(command.HashedAccountId, command.ExternalUserId), Times.Once);
            Assert.That(actual.IsValid(), Is.False);
            Assert.That(actual.ValidationDictionary, Does.Contain(new KeyValuePair<string, string>("member", "Unauthorised: User not connected to account")));
        }

        [Test]
        public async Task ThenTheCommandIsUnauthorisedIfTheUserIsNotAnOwner()
        {
            //Arrange
            var command = AddPayeToNewLegalEntityCommandObjectMother.Create(ExpectedNonOwnerUserId);

            //Act
            var actual = await _validator.ValidateAsync(command);

            //Assert
            _membershiprepository.Verify(x => x.GetCaller(command.HashedAccountId, command.ExternalUserId), Times.Once);
            Assert.That(actual.IsUnauthorized, Is.True);
        }

        [Test]
        public async Task ThenTheCommandIsValidIfAllRequiredFieldsArePopulatedAndTheUserIsAnOwner()
        {
            //Arrange
            var command = AddPayeToNewLegalEntityCommandObjectMother.Create(ExpectedOwnerUserId);
            command.AccessToken = null;
            command.RefreshToken = null;

            //Act
            var actual = await _validator.ValidateAsync(command);

            //Assert
            _membershiprepository.Verify(x => x.GetCaller(command.HashedAccountId, command.ExternalUserId), Times.Once);
            Assert.That(actual.IsValid(), Is.True);
        }
    }
}
