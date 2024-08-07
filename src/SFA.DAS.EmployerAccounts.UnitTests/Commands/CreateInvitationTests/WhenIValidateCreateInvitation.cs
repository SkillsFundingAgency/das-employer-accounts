﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.CreateInvitation;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Models.AccountTeam;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.CreateInvitationTests
{
    public class WhenIValidateCreateInvitation
    {
        private CreateInvitationCommandValidator _validator;
        private CreateInvitationCommand _createInvitationCommand;
        private Mock<IMembershipRepository> _membershipRepository;

        [SetUp]
        public void Arrange()
        {
            _createInvitationCommand = new CreateInvitationCommand
            {
                EmailOfPersonBeingInvited = "so'me@email.com",
                ExternalUserId = "123",
                HashedAccountId = "123dfg",
                NameOfPersonBeingInvited = "Test",
                RoleOfPersonBeingInvited = Role.Owner
            };

            _membershipRepository = new Mock<IMembershipRepository>();
            _membershipRepository.Setup(x => x.GetCaller(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new MembershipView { Role = Role.Owner });
            _membershipRepository.Setup(x => x.Get(It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(new TeamMember { IsUser = false });

            _validator = new CreateInvitationCommandValidator(_membershipRepository.Object);
        }

        [Test]
        public async Task ThenTrueIsReturnedIfAllFieldsArePopulated()
        {
            //Act
            var result = await _validator.ValidateAsync(_createInvitationCommand);

            //Assert
            Assert.That(result.IsValid(), Is.True);
        }

        [Test]
        public async Task ThenTheErrorDictionaryIsPopulatedWithErrorsWhenInvalid()
        {
            //Act
            var result = await _validator.ValidateAsync(new CreateInvitationCommand());

            //Assert
            Assert.That(result.IsValid(), Is.False);

            Assert.That(result.ValidationDictionary, Does.Contain(new KeyValuePair<string, string>("EmailOfPersonBeingInvited", "Enter email address")));
            Assert.That(result.ValidationDictionary, Does.Contain(new KeyValuePair<string, string>("HashedAccountId", "No HashedAccountId supplied")));
            Assert.That(result.ValidationDictionary, Does.Contain(new KeyValuePair<string, string>("NameOfPersonBeingInvited", "Enter name")));
        }

        [TestCase("notvalid")]
        [TestCase("notvalid.com")]
        [TestCase("notvalid@valid")]
        public async Task ThenTheEmailFormatIsValidatedWhenPopulatedAndReturnsFalseForNonValidEmails(string email)
        {
            //Act
            var result = await _validator.ValidateAsync(new CreateInvitationCommand
            {
                EmailOfPersonBeingInvited = email,
                ExternalUserId = "123",
                HashedAccountId = "123dfg",
                NameOfPersonBeingInvited = "Test",
                RoleOfPersonBeingInvited = Role.Owner
            });

            //Assert
            Assert.That(result.IsValid(), Is.False);
            Assert.That(result.ValidationDictionary, Does.Contain(new KeyValuePair<string, string>("EmailOfPersonBeingInvited", "Enter a valid email address")));
        }

        [Test]
        public async Task ThenTheUserIsCheckedToSeeIfTheyAreAssocaitedWithTheAccountAndTheResultIsNotValidIfTheyArent()
        {
            //Arrange
            _membershipRepository.Setup(x => x.GetCaller(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(() => null);

            //Act
            var result = await _validator.ValidateAsync(_createInvitationCommand);

            //Assert
            Assert.That(result.IsValid(), Is.False);
            Assert.That(result.IsUnauthorized, Is.True);
            Assert.That(result.ValidationDictionary, Does.Contain(new KeyValuePair<string, string>("Membership", "User is not a member of this Account")));
        }

        [Test]
        public async Task ThenTheUserIsCheckedToSeeIfTheyAreAnOwnerAndFalseIsReturnedIfTheyArent()
        {
            //Arrange
            _membershipRepository.Setup(x => x.GetCaller(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new MembershipView { Role = Role.Transactor });

            //Act
            var result = await _validator.ValidateAsync(_createInvitationCommand);

            //Assert
            Assert.That(result.IsValid(), Is.False);
            Assert.That(result.IsUnauthorized, Is.True);
            Assert.That(result.ValidationDictionary, Does.Contain(new KeyValuePair<string, string>("Membership", "User is not an Owner")));
        }

        [Test]
        public async Task ThenFalseIsReturnedIfTheEmailIsAlreadyInUse()
        {
            //Arrange
            _membershipRepository.Setup(x => x.Get(It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(new TeamMember { IsUser = true });

            //Act
            var result = await _validator.ValidateAsync(_createInvitationCommand);

            //Assert
            Assert.That(result.IsValid(), Is.False);
            Assert.That(result.ValidationDictionary, Does.Contain(new KeyValuePair<string, string>("EmailOfPersonBeingInvited", $"{_createInvitationCommand.EmailOfPersonBeingInvited} is already invited")));
        }

    }
}
