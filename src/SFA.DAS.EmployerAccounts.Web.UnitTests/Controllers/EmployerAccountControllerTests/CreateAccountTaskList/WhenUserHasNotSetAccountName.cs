﻿using System.Security.Claims;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;
using SFA.DAS.EmployerAccounts.Queries.GetAccountPayeSchemes;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAccountDetail;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAgreementsByAccountId;
using SFA.DAS.EmployerAccounts.Queries.GetUserAccounts;
using SFA.DAS.EmployerAccounts.Queries.GetUserByRef;
using SFA.DAS.EmployerAccounts.TestCommon.AutoFixture;
using SFA.DAS.EmployerAccounts.Web.RouteValues;
using SFA.DAS.Encoding;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerAccountControllerTests
{
    [TestFixture]
    public class WhenUserHasNotSetAccountName
    {
        [Test]
        [MoqAutoData]
        public async Task Then_GetCreateAccountTaskList_GetAccountWithId(
            long accountId,
            string hashedAccountId,
            string userId,
            [Frozen] Mock<IEncodingService> encodingServiceMock,
            [Frozen] Mock<IMediator> mediatorMock,
            [NoAutoProperties] EmployerAccountController controller)
        {
            // Arrange
            SetControllerContextUserIdClaim(userId, controller);
            encodingServiceMock.Setup(x => x.Decode(hashedAccountId, EncodingType.AccountId)).Returns(accountId);

            // Act
            var result = await controller.CreateAccountTaskList(hashedAccountId);

            // Assert
            mediatorMock.Verify(m => m.Send(It.Is<GetEmployerAccountDetailByIdQuery>(x => x.AccountId == accountId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        [DomainAutoData]
        public async Task And_AccountId_Not_Supplied_Then_Should_Get_PAYE(
            string userId,
            GetUserByRefResponse userByRefResponse,
            GetUserAccountsQueryResponse queryResponse,
            [Frozen] Mock<IMediator> mediatorMock,
            [NoAutoProperties] EmployerAccountController controller)
        {
            // Arrange
            SetControllerContextUserIdClaim(userId, controller);
            mediatorMock.Setup(m => m.Send(It.Is<GetUserAccountsQuery>(q => q.UserRef == userId), It.IsAny<CancellationToken>())).ReturnsAsync(queryResponse);
            mediatorMock.Setup(m => m.Send(It.Is<GetUserByRefQuery>(q => q.UserRef == userId), It.IsAny<CancellationToken>())).ReturnsAsync(userByRefResponse);

            // Act
            var result = await controller.CreateAccountTaskList(null) as ViewResult;
            var model = result.Model as OrchestratorResponse<AccountTaskListViewModel>;

            // Assert
            mediatorMock.Verify(m => m.Send(It.IsAny<GetAccountPayeSchemesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        [MoqAutoData]
        public async Task Then_CannotAddAnotherPaye(
            long agreementId,
            long accountId,
            string hashedAccountId,
            string userId,
            [NoAutoProperties] GetEmployerAgreementsByAccountIdResponse accountEmployerAgreementsResponse,
            GetUserByRefResponse userByRefResponse,
            GetEmployerAccountDetailByIdResponse accountDetailResponse,
            [Frozen] Mock<IEncodingService> encodingServiceMock,
            [Frozen] Mock<IMediator> mediatorMock,
            [NoAutoProperties] EmployerAccountController controller)
        {
            // Arrange
            encodingServiceMock.Setup(m => m.Decode(hashedAccountId, EncodingType.AccountId)).Returns(accountId);

            accountEmployerAgreementsResponse.EmployerAgreements = new List<EmployerAgreement> { new EmployerAgreement { StatusId = EmployerAgreementStatus.Pending, Id = agreementId } };
            mediatorMock.Setup(m => m.Send(It.Is<GetEmployerAgreementsByAccountIdRequest>(x => x.AccountId == accountId), It.IsAny<CancellationToken>())).ReturnsAsync(accountEmployerAgreementsResponse);
            SetControllerContextUserIdClaim(userId, controller);

            accountDetailResponse.Account.NameConfirmed = false;
            accountDetailResponse.Account.PayeSchemes = accountDetailResponse.Account.PayeSchemes.Take(1).ToList();
            mediatorMock
                .Setup(m => m.Send(It.Is<GetEmployerAccountDetailByIdQuery>(x => x.AccountId == accountId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(accountDetailResponse);
            
            mediatorMock
                .Setup(m => m.Send(It.Is<GetUserByRefQuery>(q => q.UserRef == userId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userByRefResponse);

            // Act
            var result = await controller.CreateAccountTaskList(hashedAccountId) as ViewResult;
            var model = result.Model as OrchestratorResponse<AccountTaskListViewModel>;

            // Assert
            model.Data.AddPayeRouteName.Should().Be(RouteNames.AddPayeShutter);
        }

        [Test]
        [MoqAutoData]
        public async Task And_No_Account_Then_Return_NotFound(
            string hashedAccountId,
            string userId,
            [NoAutoProperties] EmployerAccountController controller)
        {
            // Arrange
            SetControllerContextUserIdClaim(userId, controller);

            // Act
            var result = await controller.CreateAccountTaskList(hashedAccountId) as ViewResult;

            // Assert
            result.ViewName.Should().Be(ControllerConstants.NotFoundViewName);
        }

        [Test]
        [MoqAutoData]
        public async Task Then_GetCreateAccountTaskList_Sets_ViewModel_HashedAccountId(
            long agreementId,
            long accountId,
            string hashedAccountId,
            string userId,
            [NoAutoProperties] GetEmployerAgreementsByAccountIdResponse accountEmployerAgreementsResponse,
            GetUserByRefResponse userByRefResponse,
            GetEmployerAccountDetailByIdResponse accountDetailResponse,
            [Frozen] Mock<IEncodingService> encodingServiceMock,
            [Frozen] Mock<IMediator> mediatorMock,
            [NoAutoProperties] EmployerAccountController controller)
        {
            // Arrange
            encodingServiceMock.Setup(m => m.Decode(hashedAccountId, EncodingType.AccountId)).Returns(accountId);

            accountEmployerAgreementsResponse.EmployerAgreements = new List<EmployerAgreement> { new EmployerAgreement { StatusId = EmployerAgreementStatus.Pending, Id = agreementId } };
            mediatorMock.Setup(m => m.Send(It.Is<GetEmployerAgreementsByAccountIdRequest>(x => x.AccountId == accountId), It.IsAny<CancellationToken>())).ReturnsAsync(accountEmployerAgreementsResponse);
            SetControllerContextUserIdClaim(userId, controller);

            accountDetailResponse.Account.NameConfirmed = false;
            accountDetailResponse.Account.PayeSchemes = accountDetailResponse.Account.PayeSchemes.Take(1).ToList();
            mediatorMock
                .Setup(m => m.Send(It.Is<GetEmployerAccountDetailByIdQuery>(x => x.AccountId == accountId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(accountDetailResponse);
            
            mediatorMock
                .Setup(m => m.Send(It.Is<GetUserByRefQuery>(q => q.UserRef == userId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userByRefResponse);

            // Act
            var result = await controller.CreateAccountTaskList(hashedAccountId) as ViewResult;
            var model = result.Model as OrchestratorResponse<AccountTaskListViewModel>;

            // Assert
            model.Data.HashedAccountId.Should().Be(hashedAccountId);
        }

        [Test]
        [MoqAutoData]
        public async Task Then_SaveProgressRoute_Maintains_AccountContext(
            long agreementId,
            long accountId,
            string hashedAccountId,
            string userId,
            GetUserByRefResponse userByRefResponse,
            [NoAutoProperties] GetEmployerAgreementsByAccountIdResponse accountEmployerAgreementsResponse,
            GetEmployerAccountDetailByIdResponse accountDetailResponse,
            [Frozen] Mock<IEncodingService> encodingServiceMock,
            [Frozen] Mock<IMediator> mediatorMock,
            [NoAutoProperties] EmployerAccountController controller)
        {
            // Arrange
            encodingServiceMock.Setup(m => m.Decode(hashedAccountId, EncodingType.AccountId)).Returns(accountId);

            accountEmployerAgreementsResponse.EmployerAgreements = new List<EmployerAgreement> { new EmployerAgreement { StatusId = EmployerAgreementStatus.Pending, Id = agreementId } };
            mediatorMock.Setup(m => m.Send(It.Is<GetEmployerAgreementsByAccountIdRequest>(x => x.AccountId == accountId), It.IsAny<CancellationToken>())).ReturnsAsync(accountEmployerAgreementsResponse);
            SetControllerContextUserIdClaim(userId, controller);

            accountDetailResponse.Account.NameConfirmed = false;
            accountDetailResponse.Account.PayeSchemes = accountDetailResponse.Account.PayeSchemes.Take(1).ToList();
            mediatorMock
                .Setup(m => m.Send(It.Is<GetEmployerAccountDetailByIdQuery>(x => x.AccountId == accountId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(accountDetailResponse);
            
            mediatorMock
                .Setup(m => m.Send(It.Is<GetUserByRefQuery>(q => q.UserRef == userId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userByRefResponse);

            // Act
            var result = await controller.CreateAccountTaskList(hashedAccountId) as ViewResult;
            var model = result.Model as OrchestratorResponse<AccountTaskListViewModel>;

            // Assert
            model.Data.SaveProgressRouteName.Should().Be(RouteNames.PartialAccountSaveProgress);
        }

        [Test]
        [MoqAutoData]
        public async Task Then_AccountHasPayeSchemes_Should_Return_HasPaye_True(
            long agreementId,
            long accountId,
            string hashedAccountId,
            string userId,
            [NoAutoProperties] GetEmployerAgreementsByAccountIdResponse accountEmployerAgreementsResponse,
            GetUserByRefResponse userByRefResponse,
            GetEmployerAccountDetailByIdResponse accountDetailResponse,
            [Frozen] Mock<IEncodingService> encodingServiceMock,
            [Frozen] Mock<IMediator> mediatorMock,
            [NoAutoProperties] EmployerAccountController controller)
        {
            // Arrange
            encodingServiceMock.Setup(m => m.Decode(hashedAccountId, EncodingType.AccountId)).Returns(accountId);

            accountEmployerAgreementsResponse.EmployerAgreements = new List<EmployerAgreement>
            {
                new()
                {
                    StatusId = EmployerAgreementStatus.Pending, 
                    Id = agreementId,
                    Acknowledged = false
                }
            };
            mediatorMock.Setup(m => m.Send(It.Is<GetEmployerAgreementsByAccountIdRequest>(x => x.AccountId == accountId), It.IsAny<CancellationToken>())).ReturnsAsync(accountEmployerAgreementsResponse);
            SetControllerContextUserIdClaim(userId, controller);

            accountDetailResponse.Account.NameConfirmed = false;
            accountDetailResponse.Account.PayeSchemes = accountDetailResponse.Account.PayeSchemes.Take(1).ToList();
            mediatorMock
                .Setup(m => m.Send(It.Is<GetEmployerAccountDetailByIdQuery>(x => x.AccountId == accountId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(accountDetailResponse);
            
            mediatorMock
                .Setup(m => m.Send(It.Is<GetUserByRefQuery>(q => q.UserRef == userId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userByRefResponse);

            // Act
            var result = await controller.CreateAccountTaskList(hashedAccountId) as ViewResult;
            var model = result.Model as OrchestratorResponse<AccountTaskListViewModel>;

            // Assert
            Assert.That(model, Is.Not.Null);
            model.Data.HasPayeScheme.Should().BeTrue();
            model.Data.CompletedSections.Should().Be(2);
        }

        private static void SetControllerContextUserIdClaim(string userId, EmployerAccountController controller)
        {
            var claims = new List<Claim> { new Claim(ControllerConstants.UserRefClaimKeyName, userId) };
            var claimsIdentity = new ClaimsIdentity(claims);
            var user = new ClaimsPrincipal(claimsIdentity);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
        }
    }
}
