using System.Security.Claims;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAccountDetail;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAgreementsByAccountId;
using SFA.DAS.EmployerAccounts.Queries.GetUserByRef;
using SFA.DAS.Encoding;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerAccountControllerTests.CreateAccountTaskList
{
    [TestFixture]
    public class WhenUserHasNotSignedLegalAgreement
    {
        [Test]
        [MoqAutoData]
        public async Task Then_HashedAccountId_IsDecoded(
            string hashedAccountId,
            string userId,
            [Frozen] Mock<IEncodingService> encodingServiceMock,
            [NoAutoProperties] EmployerAccountController controller)
        {
            // Arrange
            SetControllerContextUserIdClaim(userId, controller);

            // Act
            _ = await controller.CreateAccountTaskList(hashedAccountId);

            // Assert
            encodingServiceMock.Verify(m => m.Decode(hashedAccountId, EncodingType.AccountId), Times.Once);
        }

        [Test]
        [MoqAutoData]
        public async Task Then_GetAccountAgreements(
            string hashedAccountId,
            long accountId,
            string userId,
            [Frozen] Mock<IEncodingService> encodingServiceMock,
            [Frozen] Mock<IMediator> mediatorMock,
            [NoAutoProperties] EmployerAccountController controller)
        {
            // Arrange
            encodingServiceMock.Setup(m => m.Decode(hashedAccountId, EncodingType.AccountId)).Returns(accountId);
            SetControllerContextUserIdClaim(userId, controller);

            // Act
            _ = await controller.CreateAccountTaskList(hashedAccountId);

            // Assert
            mediatorMock.Verify(m => m.Send(It.Is<GetEmployerAgreementsByAccountIdRequest>(x => x.AccountId == accountId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        [MoqAutoData]
        public async Task Then_AccountHasNameConfirmed_Should_Return_NameConfirmed_True(
            long agreementId,
            long accountId,
            string hashedAccountId,
            string userId,
            GetUserByRefResponse userByRefResponse,
            [NoAutoProperties] GetEmployerAgreementsByAccountIdResponse accountEmployerAgreementsResponse,
            GetEmployerAccountDetailByHashedIdResponse accountDetailResponse,
            [Frozen] Mock<IUrlActionHelper> urlHelperMock,
            [Frozen] Mock<IEncodingService> encodingServiceMock,
            [Frozen] Mock<IMediator> mediatorMock,
            [NoAutoProperties] EmployerAccountController controller)
        {
            // Arrange
            encodingServiceMock.Setup(m => m.Decode(hashedAccountId, EncodingType.AccountId)).Returns(accountId);
            accountEmployerAgreementsResponse.EmployerAgreements = new List<EmployerAgreement>
            {
                new EmployerAgreement
                {
                    StatusId = EmployerAgreementStatus.Pending, 
                    Id = agreementId, 
                    Acknowledged = false
                }
            };
            
            mediatorMock.Setup(m => m.Send(It.Is<GetEmployerAgreementsByAccountIdRequest>(x => x.AccountId == accountId), It.IsAny<CancellationToken>())).ReturnsAsync(accountEmployerAgreementsResponse); 
            SetControllerContextUserIdClaim(userId, controller);
            
            mediatorMock
                .Setup(m => m.Send(It.Is<GetUserByRefQuery>(q => q.UserRef == userId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userByRefResponse);

            accountDetailResponse.Account.PayeSchemes = accountDetailResponse.Account.PayeSchemes.Take(1).ToList();
            accountDetailResponse.Account.NameConfirmed = true;

            mediatorMock
                .Setup(m => m.Send(It.Is<GetEmployerAccountDetailByHashedIdQuery>(x => x.HashedAccountId == hashedAccountId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(accountDetailResponse);

            // Act
            var result = await controller.CreateAccountTaskList(hashedAccountId) as ViewResult;
            var model = result.Model as OrchestratorResponse<AccountTaskListViewModel>;

            // Assert
            Assert.IsNotNull(model);
            model.Data.NameConfirmed.Should().BeTrue();
            model.Data.CompletedSections.Should().Be(3);
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
