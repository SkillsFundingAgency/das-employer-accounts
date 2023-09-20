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

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerAccountControllerTests
{
    [TestFixture]
    public class WhenUserHasNotAcknowledgedTrainingProvider
    {
        [Test]
        [MoqAutoData]
        public async Task Then_AgreementAcknowledged_Should_Return_NameConfirmed_True(
            long agreementId,
            long accountId,
            string hashedAccountId,
            string userId,
            [NoAutoProperties] GetEmployerAgreementsByAccountIdResponse accountEmployerAgreementsResponse,
            GetEmployerAccountDetailByHashedIdResponse accountDetailResponse,
            GetUserByRefResponse userByRefResponse,
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
                    Id = agreementId, Acknowledged = true
                }
            };
            
            mediatorMock
                .Setup(m => m.Send(It.Is<GetEmployerAgreementsByAccountIdRequest>(x => x.AccountId == accountId),
                    It.IsAny<CancellationToken>())).ReturnsAsync(accountEmployerAgreementsResponse);
            SetControllerContextUserIdClaim(userId, controller);
            
            mediatorMock
                .Setup(m => m.Send(It.Is<GetUserByRefQuery>(q => q.UserRef == userId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userByRefResponse);

            accountDetailResponse.Account.PayeSchemes = accountDetailResponse.Account.PayeSchemes.Take(1).ToList();
            accountDetailResponse.Account.NameConfirmed = true;

            mediatorMock
                .Setup(m => m.Send(
                    It.Is<GetEmployerAccountDetailByHashedIdQuery>(x => x.HashedAccountId == hashedAccountId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(accountDetailResponse);

            // Act
            var result = await controller.CreateAccountTaskList(hashedAccountId) as ViewResult;
            var model = result.Model as OrchestratorResponse<AccountTaskListViewModel>;

            // Assert
            Assert.IsNotNull(model);
            model.Data.AgreementAcknowledged.Should().BeTrue();
            model.Data.CompletedSections.Should().Be(4);
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