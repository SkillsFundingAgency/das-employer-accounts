﻿using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Models.AccountTeam;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAccount;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetEmployerAccountTests
{
    public class WhenIValidateTheGetAccountByIdRequest
    {
        [Test]
        [MoqInlineAutoData(0)]
        [MoqInlineAutoData(-1)]
        [MoqInlineAutoData(-999)]
        public async Task ThenTheRequestIsNotValidIfAccountIdInvalidArentPopulatedAndTheRepositoryIsNotCalled(
            long accountId,
            GetEmployerAccountByIdQuery query,
            GetEmployerAccountByIdValidator validator)
        {
            //Arrange
            query.AccountId = accountId;

            //Act
            var actual = await validator.ValidateAsync(query);

            //Assert
            Assert.That(actual.IsValid(), Is.False);
            Assert.That(actual.ValidationDictionary, Does.Contain(new KeyValuePair<string, string>("AccountId", "Account ID has not been supplied")));
        }

        [Test, MoqAutoData]
        public async Task ThenTheRequestIsNotValidIfUserIdNotSupplied_TheRepositoryIsNotCalled(
            GetEmployerAccountByIdQuery query,
            GetEmployerAccountByIdValidator validator)
        {
            //Arrange
            query.UserId = string.Empty;

            //Act
            var actual = await validator.ValidateAsync(query);

            //Assert
            Assert.That(actual.IsValid(), Is.False);
            Assert.That(actual.ValidationDictionary, Does.Contain(new KeyValuePair<string, string>("UserId", "User ID has not been supplied")));
        }

        [Test, MoqAutoData]
        public async Task WhenTheRequestHasValidAccountId_TheMembershipIsFetched(
           [Frozen] Mock<IMembershipRepository> membershipRepoMock,
           GetEmployerAccountByIdQuery query,
           GetEmployerAccountByIdValidator validator)
        {
            //Arrange

            //Act
            var actual = await validator.ValidateAsync(query);

            //Assert
            membershipRepoMock.Verify(mock => mock.GetCaller(It.Is<long>(l => l == query.AccountId), It.Is<string>(s => s == query.UserId)));
        }

        [Test]
        [MoqInlineAutoData(Role.Viewer)]
        [MoqInlineAutoData(Role.Transactor)]
        [MoqInlineAutoData(Role.Owner)]
        public async Task WhenAccountMemberThenAuthorized(
            Role userRole,
            [Frozen] Mock<IMembershipRepository> membershipRepoMock,
            GetEmployerAccountByIdQuery query,
            GetEmployerAccountByIdValidator validator)
        {
            //Arrange
            membershipRepoMock.Setup(x => x.GetCaller(It.Is<long>(l => l == query.AccountId), It.Is<string>(s => s == query.UserId))).ReturnsAsync(new MembershipView { Role = userRole });

            //Act
            var actual = await validator.ValidateAsync(query);

            //Assert
            Assert.That(actual.IsValid(), Is.True);
            Assert.That(actual.IsUnauthorized, Is.False);
        }

        [Test, MoqAutoData]
        public async Task ThenTheRequestIsMarkedAsInvalidIfTheUserDoesNotExist(
            [Frozen] Mock<IMembershipRepository> membershipRepoMock,
            GetEmployerAccountByIdQuery query,
            GetEmployerAccountByIdValidator validator)
        {
            //Arrange
            membershipRepoMock.Setup(x => x.GetCaller(It.Is<long>(l => l == query.AccountId), It.Is<string>(s => s == query.UserId))).ReturnsAsync(() => null);

            //Act
            var actual = await validator.ValidateAsync(query);

            //Assert
            Assert.That(actual.ValidationDictionary, Does.Contain(new KeyValuePair<string, string>("membership", "Unauthorised: User not connected to account")));
            Assert.That(actual.IsValid(), Is.False);
            Assert.That(actual.IsUnauthorized, Is.True);
        }
    }
}
