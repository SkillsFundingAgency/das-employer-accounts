using System.Security.Claims;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerAccounts.Queries.GetCreateAccountTaskList;
using SFA.DAS.EmployerAccounts.Queries.GetUserAccounts;
using SFA.DAS.EmployerAccounts.Queries.GetUserByRef;
using SFA.DAS.EmployerAccounts.TestCommon.AutoFixture;
using SFA.DAS.EmployerAccounts.Web.RouteValues;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerAccountControllerTests.CreateAccountTaskList;

[TestFixture]
public class WhenUserHasNotAddedPayeAndOrganisation
{
    [Test]
    [DomainAutoData]
    public async Task Then_CannotAddAnotherPaye(
        string userId,
        GetUserByRefResponse userByRefResponse,
        GetUserAccountsQueryResponse queryResponse,
        [Frozen] Mock<IMediator> mediatorMock,
        [NoAutoProperties] EmployerAccountController controller)
    {
        // Arrange
        queryResponse.Accounts.AccountList.Clear();
        SetControllerContextUserIdClaim(userId, controller);
        mediatorMock.Setup(m => m.Send(It.Is<GetUserAccountsQuery>(q => q.UserRef == userId), It.IsAny<CancellationToken>())).ReturnsAsync(queryResponse);
        mediatorMock
            .Setup(m => m.Send(It.Is<GetUserByRefQuery>(q => q.UserRef == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userByRefResponse);
            
        // Act
        var result = await controller.CreateAccountTaskList(string.Empty) as ViewResult;
        var model = result.Model as OrchestratorResponse<AccountTaskListViewModel>;

        // Assert
        model.Data.AddPayeRouteName.Should().Be(RouteNames.EmployerAccountPayBillTriage);
    }

    [Test]
    [DomainAutoData]
    public async Task Then_SetEditUserDetailsUrl(
        string userId,
        Uri editUserDetailsUri,
        [Frozen] Mock<IUrlActionHelper> urlHelperMock,
        [Frozen] Mock<IMediator> mediatorMock,
        [NoAutoProperties] EmployerAccountController controller,
        GetCreateAccountTaskListQueryResponse taskListResponse,
        Mock<IEncodingService> encodingServiceMock,
        long accountId,
        string hashedAccountId)
    {
        // Arrange
  
        encodingServiceMock.Setup(m => m.Decode(hashedAccountId, EncodingType.AccountId)).Returns(accountId);
        urlHelperMock.Setup(m => m.EmployerProfileEditUserDetails(It.IsAny<string>())).Returns(editUserDetailsUri.AbsoluteUri);
  
        SetControllerContextUserIdClaim(userId, controller);

        mediatorMock
            .Setup(m => m.Send(It.Is<GetCreateAccountTaskListQuery>(x =>
                    x.AccountId == accountId
                    && x.HashedAccountId == hashedAccountId
                    && x.UserRef == userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskListResponse);
            
        // Act
        var result = await controller.CreateAccountTaskList(hashedAccountId) as ViewResult;
        var model = result.Model as OrchestratorResponse<AccountTaskListViewModel>;

        // Assert
        model.Data.EditUserDetailsUrl.Should().Be($"{editUserDetailsUri.AbsoluteUri}?firstName={taskListResponse.UserFirstName}&lastName={taskListResponse.UserLastName}");
    }

    [Test]
    [DomainAutoData]
    public async Task Then_SaveProgressRoute_Without_AccountContext(
        string userId,
        GetUserByRefResponse userByRefResponse,
        GetUserAccountsQueryResponse queryResponse,
        [Frozen] Mock<IMediator> mediatorMock,
        Mock<IEncodingService> encodingServiceMock,
        [NoAutoProperties] EmployerAccountController controller)
    {
        // Arrange
        queryResponse.Accounts.AccountList.Clear();
        SetControllerContextUserIdClaim(userId, controller);
        mediatorMock.Setup(m => m.Send(It.Is<GetUserAccountsQuery>(q => q.UserRef == userId), It.IsAny<CancellationToken>())).ReturnsAsync(queryResponse);
        mediatorMock
            .Setup(m => m.Send(It.Is<GetUserByRefQuery>(q => q.UserRef == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userByRefResponse);
        
        //encodingServiceMock.Setup(m => m.Decode(hashedAccountId, EncodingType.AccountId)).Returns(accountId);
            
        // Act
        var result = await controller.CreateAccountTaskList(string.Empty) as ViewResult;
        var model = result.Model as OrchestratorResponse<AccountTaskListViewModel>;

        // Assert
        model.Data.SaveProgressRouteName.Should().Be(RouteNames.NewAccountSaveProgress);
    }

    private static void SetControllerContextUserIdClaim(string userId, EmployerAccountController controller)
    {
        var claims = new List<Claim> { new Claim(ControllerConstants.UserRefClaimKeyName, userId) };
        var claimsIdentity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(claimsIdentity);
        controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
    }
}