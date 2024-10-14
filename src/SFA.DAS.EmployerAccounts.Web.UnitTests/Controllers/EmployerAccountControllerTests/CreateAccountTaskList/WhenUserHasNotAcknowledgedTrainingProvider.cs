using System.Security.Claims;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerAccounts.Queries.GetCreateAccountTaskList;
using SFA.DAS.Encoding;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerAccountControllerTests.CreateAccountTaskList;

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
        GetCreateAccountTaskListQueryResponse taskListResponse,
        [Frozen] Mock<IEncodingService> encodingServiceMock,
        [Frozen] Mock<IMediator> mediatorMock,
        [NoAutoProperties] EmployerAccountController controller)
    {
        // Arrange
        encodingServiceMock.Setup(m => m.Decode(hashedAccountId, EncodingType.AccountId)).Returns(accountId);

        taskListResponse.HasProviders = false;
        taskListResponse.HasProviderPermissions = false;
        
        mediatorMock
            .Setup(m => m.Send(It.Is<GetCreateAccountTaskListQuery>(x =>
                    x.AccountId == accountId
                    && x.HashedAccountId == hashedAccountId
                    && x.UserRef == userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskListResponse);
        
        SetControllerContextUserIdClaim(userId, controller);
        
        // Act
        var result = await controller.CreateAccountTaskList(hashedAccountId) as ViewResult;
        var model = result.Model as OrchestratorResponse<AccountTaskListViewModel>;

        // Assert
        model.Should().NotBeNull();
        model.Data.AgreementAcknowledged.Should().BeTrue();
        model.Data.CompletedSections.Should().Be(4);
    }

    private static void SetControllerContextUserIdClaim(string userId, EmployerAccountController controller)
    {
        var claims = new List<Claim> { new(ControllerConstants.UserRefClaimKeyName, userId) };
        var claimsIdentity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(claimsIdentity);
        controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
    }
}