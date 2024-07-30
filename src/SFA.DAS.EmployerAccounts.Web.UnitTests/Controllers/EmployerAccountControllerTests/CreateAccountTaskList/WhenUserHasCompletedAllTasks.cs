using System.Security.Claims;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerAccounts.Queries.GetCreateAccountTaskList;
using SFA.DAS.EmployerAccounts.Web.RouteValues;
using SFA.DAS.Encoding;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerAccountControllerTests.CreateAccountTaskList;

[TestFixture]
public class WhenUserHasCompletedAllTasks
{
    [Test]
    [MoqAutoData]
    public async Task Then_ShouldRedirect(
        long accountId,
        string hashedAccountId,
        string userId,
        [Frozen] Mock<IEncodingService> encodingServiceMock,
        [Frozen] Mock<IMediator> mediatorMock,
        [NoAutoProperties] EmployerAccountController controller,
        GetCreateAccountTaskListQueryResponse taskListResponse)
    {
        // Arrange
        encodingServiceMock.Setup(m => m.TryDecode(hashedAccountId, EncodingType.AccountId, out accountId)).Returns(true);

        taskListResponse.NameConfirmed = true;
        taskListResponse.AgreementAcknowledged = true;
        taskListResponse.AddTrainingProviderAcknowledged = true;
        taskListResponse.HasProviders = true;
        taskListResponse.HasProviderPermissions = true;
        taskListResponse.HasSignedAgreement = true;

        mediatorMock
            .Setup(m => m.Send(It.Is<GetCreateAccountTaskListQuery>(x =>
                    x.AccountId == accountId
                    && x.HashedAccountId == hashedAccountId
                    && x.UserRef == userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskListResponse)
            .Verifiable();

        SetControllerContextUserIdClaim(userId, controller);

        // Act
        var result = await controller.CreateAccountTaskList(hashedAccountId) as RedirectToRouteResult;

        // Assert
        result.RouteName.Should().Be(RouteNames.EmployerTeamIndex);
        
        mediatorMock.Verify();
        mediatorMock.VerifyNoOtherCalls();
    }

    private static void SetControllerContextUserIdClaim(string userId, EmployerAccountController controller)
    {
        var claims = new List<Claim> { new(ControllerConstants.UserRefClaimKeyName, userId) };
        var claimsIdentity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(claimsIdentity);
        controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
    }
}