using System.Security.Claims;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerAccounts.Commands.AcknowledgeTrainingProviderTask;
using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAccountDetail;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAgreementsByAccountId;
using SFA.DAS.EmployerAccounts.Queries.GetUserByRef;
using SFA.DAS.EmployerAccounts.Web.RouteValues;
using SFA.DAS.Encoding;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerAccountControllerTests.CreateAccountTaskList;

[TestFixture]
public class WhenUserHasCompletedAllTasks
{
    [Test]
    [MoqAutoData]
    public async Task PermissionsAdded_Then_ShouldRedirect(
        long agreementId,
        long accountId,
        string hashedAccountId,
        string userId,
        GetUserByRefResponse userByRefResponse,
        [NoAutoProperties] GetEmployerAgreementsByAccountIdResponse accountEmployerAgreementsResponse,
        GetEmployerAccountDetailByIdResponse accountDetailResponse,
        [Frozen] Mock<IEmployerAccountService> employerAccountServiceMock,
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
                Acknowledged = true
            }
        };

        employerAccountServiceMock.Setup(m => m.GetEmployerAccountTaskList(accountId, hashedAccountId)).ReturnsAsync(
            new EmployerAccountTaskList()
            {
                HasProviders = true,
                HasProviderPermissions = true,
            });
        
        mediatorMock
            .Setup(m => m.Send(It.Is<GetEmployerAgreementsByAccountIdRequest>(x => x.AccountId == accountId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(accountEmployerAgreementsResponse)
            .Verifiable();
        SetControllerContextUserIdClaim(userId, controller);

        mediatorMock
            .Setup(m => m.Send(It.Is<GetUserByRefQuery>(q => q.UserRef == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userByRefResponse)
            .Verifiable();

        accountDetailResponse.Account.AddTrainingProviderAcknowledged = true;
        accountDetailResponse.Account.PayeSchemes = accountDetailResponse.Account.PayeSchemes.Take(1).ToList();
        accountDetailResponse.Account.NameConfirmed = true;

        mediatorMock
            .Setup(m => m.Send(
                It.Is<GetEmployerAccountDetailByHashedIdQuery>(x => x.HashedAccountId == hashedAccountId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(accountDetailResponse)
            .Verifiable();

        // Act
        var result = await controller.CreateAccountTaskList(hashedAccountId) as RedirectToRouteResult;

        // Assert
        mediatorMock.Verify();
        mediatorMock.VerifyNoOtherCalls();
        result.RouteName.Should().Be(RouteNames.EmployerTeamIndex);
    }
    
    [Test]
    [MoqAutoData]
    public async Task PermissionsAdded_And_TrainingProviderAcknowledgedFalse_Then_Should_SetAcknowledged_And_Redirect(
        long agreementId,
        long accountId,
        string hashedAccountId,
        string userId,
        GetUserByRefResponse userByRefResponse,
        [NoAutoProperties] GetEmployerAgreementsByAccountIdResponse accountEmployerAgreementsResponse,
        GetEmployerAccountDetailByHashedIdResponse accountDetailResponse,
        [Frozen] Mock<IEmployerAccountService> employerAccountServiceMock,
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
                Acknowledged = true
            }
        };

        employerAccountServiceMock.Setup(m => m.GetEmployerAccountTaskList(accountId, hashedAccountId)).ReturnsAsync(
            new EmployerAccountTaskList()
            {
                HasProviders = true,
                HasProviderPermissions = true,
            });
        
        mediatorMock
            .Setup(m => m.Send(It.Is<GetEmployerAgreementsByAccountIdRequest>(x => x.AccountId == accountId),
                It.IsAny<CancellationToken>())).ReturnsAsync(accountEmployerAgreementsResponse);
        SetControllerContextUserIdClaim(userId, controller);

        mediatorMock
            .Setup(m => m.Send(It.Is<GetUserByRefQuery>(q => q.UserRef == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userByRefResponse);

        accountDetailResponse.Account.AddTrainingProviderAcknowledged = false;
        accountDetailResponse.Account.PayeSchemes = accountDetailResponse.Account.PayeSchemes.Take(1).ToList();
        accountDetailResponse.Account.NameConfirmed = true;

        mediatorMock
            .Setup(m => m.Send(
                It.Is<GetEmployerAccountDetailByIdQuery>(x => x.AccountId == accountId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(accountDetailResponse);

        // Act
        var result = await controller.CreateAccountTaskList(hashedAccountId) as RedirectToRouteResult;

        // Assert
        result.RouteName.Should().Be(RouteNames.EmployerTeamIndex);
        mediatorMock
            .Verify(x => x.Send(
                It.IsAny<AcknowledgeTrainingProviderTaskCommand>(),
                It.IsAny<CancellationToken>()));
    }
    
    [Test]
    [MoqAutoData]
    public async Task TrainingProviderAcknowledged_Then_ShouldRedirect(
        long agreementId,
        long accountId,
        string hashedAccountId,
        string userId,
        GetUserByRefResponse userByRefResponse,
        [NoAutoProperties] GetEmployerAgreementsByAccountIdResponse accountEmployerAgreementsResponse,
        GetEmployerAccountDetailByIdResponse accountDetailResponse,
        [Frozen] Mock<IEmployerAccountService> employerAccountServiceMock,
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
                Acknowledged = true
            }
        };

        employerAccountServiceMock.Setup(m => m.GetEmployerAccountTaskList(accountId, hashedAccountId)).ReturnsAsync(
            new EmployerAccountTaskList()
            {
                HasProviders = false,
                HasProviderPermissions = false,
            });
        
        mediatorMock
            .Setup(m => m.Send(It.Is<GetEmployerAgreementsByAccountIdRequest>(x => x.AccountId == accountId),
                It.IsAny<CancellationToken>())).ReturnsAsync(accountEmployerAgreementsResponse);
        SetControllerContextUserIdClaim(userId, controller);

        mediatorMock
            .Setup(m => m.Send(It.Is<GetUserByRefQuery>(q => q.UserRef == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userByRefResponse);

        accountDetailResponse.Account.PayeSchemes = accountDetailResponse.Account.PayeSchemes.Take(1).ToList();
        accountDetailResponse.Account.NameConfirmed = true;
        accountDetailResponse.Account.AddTrainingProviderAcknowledged = true;

        mediatorMock
            .Setup(m => m.Send(
                It.Is<GetEmployerAccountDetailByIdQuery>(x => x.AccountId == accountId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(accountDetailResponse);

        // Act
        var result = await controller.CreateAccountTaskList(hashedAccountId) as RedirectToRouteResult;

        // Assert
        result.RouteName.Should().Be(RouteNames.EmployerTeamIndex);
    }

    private static void SetControllerContextUserIdClaim(string userId, EmployerAccountController controller)
    {
        var claims = new List<Claim> { new Claim(ControllerConstants.UserRefClaimKeyName, userId) };
        var claimsIdentity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(claimsIdentity);
        controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
    }
}