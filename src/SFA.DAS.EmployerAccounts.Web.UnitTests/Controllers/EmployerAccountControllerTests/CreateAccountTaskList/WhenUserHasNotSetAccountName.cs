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

public class WhenUserHasNotSetAccountName
{
    [Test]
    [MoqAutoData]
    public async Task Then_CannotAddAnotherPaye(
        long accountId,
        string hashedAccountId,
        string userId,
        [Frozen] Mock<IEncodingService> encodingServiceMock,
        [Frozen] Mock<IMediator> mediatorMock,
        [NoAutoProperties] EmployerAccountController controller,
        GetCreateAccountTaskListQueryResponse taskListResponse)
    {
        // Arrange

        encodingServiceMock.Setup(m => m.Decode(hashedAccountId, EncodingType.AccountId)).Returns(accountId);

        taskListResponse.NameConfirmed = false;
        taskListResponse.HasProviderPermissions = false;
        taskListResponse.AddTrainingProviderAcknowledged = false;
        taskListResponse.HashedAccountId = hashedAccountId;

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
        model.Data.AddPayeRouteName.Should().Be(RouteNames.AddPayeShutter);
    }

    [Test]
    [MoqAutoData]
    public async Task And_No_Account_Then_Return_Ok(
        string hashedAccountId,
        string userId,
        long accountId,
        [Frozen] Mock<IMediator> mediatorMock,
        [Frozen] Mock<IEncodingService> encodingServiceMock,
        [NoAutoProperties] EmployerAccountController controller)
    {
        encodingServiceMock.Setup(m => m.Decode(hashedAccountId, EncodingType.AccountId)).Returns(accountId);

        // Arrange
        mediatorMock
            .Setup(m => m.Send(It.Is<GetCreateAccountTaskListQuery>(x =>
                    x.AccountId == accountId
                    && x.HashedAccountId == hashedAccountId
                    && x.UserRef == userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => null);

        SetControllerContextUserIdClaim(userId, controller);

        // Act
        var result = await controller.CreateAccountTaskList(hashedAccountId) as ViewResult;

        // Assert
        result.ViewName.Should().Be(nameof(CreateAccountTaskList));
    }

    [Test]
    [MoqAutoData]
    public async Task Then_SaveProgressRoute_Maintains_AccountContext(
        long accountId,
        string hashedAccountId,
        string userId,
        [Frozen] Mock<IEncodingService> encodingServiceMock,
        [Frozen] Mock<IMediator> mediatorMock,
        [NoAutoProperties] EmployerAccountController controller,
        GetCreateAccountTaskListQueryResponse taskListResponse)
    {
        // Arrange
        taskListResponse.NameConfirmed = false;
        taskListResponse.HasProviderPermissions = false;
        taskListResponse.AddTrainingProviderAcknowledged = false;

        encodingServiceMock.Setup(m => m.Decode(hashedAccountId, EncodingType.AccountId)).Returns(accountId);

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
        model.Data.SaveProgressRouteName.Should().Be(RouteNames.PartialAccountSaveProgress);
    }

    private static void SetControllerContextUserIdClaim(string userId, EmployerAccountController controller)
    {
        var claims = new List<Claim> { new(ControllerConstants.UserRefClaimKeyName, userId) };
        var claimsIdentity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(claimsIdentity);
        controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
    }
}