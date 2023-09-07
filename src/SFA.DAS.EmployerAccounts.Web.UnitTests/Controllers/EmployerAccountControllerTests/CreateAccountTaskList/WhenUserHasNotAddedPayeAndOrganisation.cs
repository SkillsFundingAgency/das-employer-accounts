using System.Security.Claims;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Queries.GetAccountPayeSchemes;
using SFA.DAS.EmployerAccounts.Queries.GetUserAccounts;
using SFA.DAS.EmployerAccounts.Queries.GetUserByRef;
using SFA.DAS.EmployerAccounts.TestCommon.AutoFixture;
using SFA.DAS.EmployerAccounts.Web.RouteValues;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerAccountControllerTests.CreateAccountTaskList
{
    [TestFixture]
    public class WhenUserHasNotAddedPayeAndOrganisation
    {
        [Test]
        [DomainAutoData]
        public async Task Then_GetUserAccounts(
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
            var result = await controller.CreateAccountTaskList(null) as ActionResult;

            // Assert
            mediatorMock.Verify(m => m.Send(It.Is<GetUserAccountsQuery>(q => q.UserRef == userId), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Test]
        [DomainAutoData]
        public async Task Then_EditUserDetailsLinkSet(
            string userId,
            GetUserByRefResponse userByRefResponse,
            GetUserAccountsQueryResponse queryResponse,
            [Frozen] Mock<IUrlActionHelper> urlHelperMock,
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
            urlHelperMock.Setup(m => m.EmployerProfileAddUserDetails("/user/edit-user-details"))
                .Returns((string path) => $"http://testpath.gov.uk{path}");
            
            // Act
            var result = await controller.CreateAccountTaskList(null) as ViewResult;

            // Assert
            var viewModel = result.Model as OrchestratorResponse<AccountTaskListViewModel>;
            viewModel.Data.EditUserDetailsUrl
                .Should()
                .Contain($"user/edit-user-details?firstName={userByRefResponse.User.FirstName}&lastName={userByRefResponse.User.LastName}");
        }

        [Test]
        [DomainAutoData]
        public async Task Then_Should_Not_Get_PAYE(
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
            var result = await controller.CreateAccountTaskList(null) as ViewResult;
            var model = result.Model as OrchestratorResponse<AccountTaskListViewModel>;

            // Assert
            mediatorMock.Verify(m => m.Send(It.IsAny<GetAccountPayeSchemesQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        [DomainAutoData]
        public async Task Then_HasPaye_False(
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
            var result = await controller.CreateAccountTaskList(null) as ViewResult;
            var model = result.Model as OrchestratorResponse<AccountTaskListViewModel>;

            // Assert
            Assert.IsNotNull(model);
            model.Data.HasPayeScheme.Should().BeFalse();
            model.Data.CompletedSections.Should().Be(1);
        }

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
            GetUserByRefResponse userByRefResponse,
            Uri editUserDetailsUri,
            GetUserAccountsQueryResponse queryResponse,
            [Frozen] Mock<IUrlActionHelper> urlHelperMock,
            [Frozen] Mock<IMediator> mediatorMock,
            [NoAutoProperties] EmployerAccountController controller)
        {
            // Arrange
            queryResponse.Accounts.AccountList.Clear();
            SetControllerContextUserIdClaim(userId, controller);
            mediatorMock.Setup(m => m.Send(It.Is<GetUserAccountsQuery>(q => q.UserRef == userId), It.IsAny<CancellationToken>())).ReturnsAsync(queryResponse);
            urlHelperMock.Setup(m => m.EmployerProfileAddUserDetails(It.IsAny<string>())).Returns(editUserDetailsUri.AbsoluteUri);
            mediatorMock
                .Setup(m => m.Send(It.Is<GetUserByRefQuery>(q => q.UserRef == userId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userByRefResponse);
            
            // Act
            var result = await controller.CreateAccountTaskList(string.Empty) as ViewResult;
            var model = result.Model as OrchestratorResponse<AccountTaskListViewModel>;

            // Assert
            model.Data.EditUserDetailsUrl.Should().Be($"{editUserDetailsUri.AbsoluteUri}?firstName={userByRefResponse.User.FirstName}&lastName={userByRefResponse.User.LastName}");
        }

        [Test]
        [DomainAutoData]
        public async Task Then_SaveProgressRoute_Without_AccountContext(
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
}
