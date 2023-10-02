using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using SFA.DAS.EmployerAccounts.Commands.AcknowledgeTrainingProviderTask;
using SFA.DAS.EmployerAccounts.Commands.CreateAccountComplete;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAccount;
using SFA.DAS.EmployerAccounts.Web.RouteValues;
using SFA.DAS.Encoding;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerAccountControllerTests.AddTrainingProviderTriage;

class WhenIChooseNotToAddATrainingProvider : EmployerAccountControllerTestsBase
{
    [Test, MoqAutoData]
    public async Task ThenHashedAccountIdIsDecoded(
        long accountId,
        string userId,
        string hashedAccountId,
        Uri providersUri,
        [NoAutoProperties] GetEmployerAccountByIdResponse employerAccountByIdResponse,
        [Frozen] Mock<IEncodingService> encodingService,
        [Frozen] Mock<IMediator> mediator,
        Mock<IUrlActionHelper> urlActionHelper,
        [NoAutoProperties] EmployerAccountController controller)
    {
        // Arrange
        SetControllerContextUserIdClaim(userId, controller);
        employerAccountByIdResponse.Account = new Account { Name = "some account name"};

        encodingService.Setup(e => e.Decode(hashedAccountId, EncodingType.AccountId)).Returns(accountId);
        
        mediator
            .Setup(m => m.Send(
                It.Is<GetEmployerAccountByIdQuery>(q => q.UserId == userId && q.AccountId == accountId), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(employerAccountByIdResponse);
        
        //Act
        var result = (await controller.AddTrainingProviderTriage(hashedAccountId, 2, urlActionHelper.Object)) as RedirectToRouteResult;

        //Assert
        encodingService.Verify(e => e.Decode(hashedAccountId, EncodingType.AccountId), Times.Once);
    }
    
    [Test, MoqAutoData]
    public async Task ThenTrainingProviderStepIsAcknowledged(
        long accountId,
        string userId,
        string hashedAccountId,
        Uri providersUri,
        [NoAutoProperties] GetEmployerAccountByIdResponse employerAccountByIdResponse,
        [Frozen] Mock<IEncodingService> encodingService,
        [Frozen] Mock<IMediator> mediator,
        Mock<IUrlActionHelper> urlActionHelper,
        [NoAutoProperties] EmployerAccountController controller)
    {
        // Arrange
        SetControllerContextUserIdClaim(userId, controller);
        employerAccountByIdResponse.Account = new Account { Name = "some account name"};

        encodingService.Setup(e => e.Decode(hashedAccountId, EncodingType.AccountId)).Returns(accountId);
        
        mediator
            .Setup(m => m.Send(
                It.Is<GetEmployerAccountByIdQuery>(q => q.UserId == userId && q.AccountId == accountId), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(employerAccountByIdResponse);
        
        //Act
        var result = (await controller.AddTrainingProviderTriage(hashedAccountId, 2, urlActionHelper.Object)) as RedirectToRouteResult;

        //Assert
        mediator
            .Verify(m => m.Send(
                It.Is<AcknowledgeTrainingProviderTaskCommand>(c => c.AccountId == accountId),
                It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Test, MoqAutoData]
    public async Task ThenAccountNameIsFetched(
        long accountId,
        string userId,
        string hashedAccountId,
        Uri providersUri,
        [NoAutoProperties] GetEmployerAccountByIdResponse employerAccountByIdResponse,
        [Frozen] Mock<IEncodingService> encodingService,
        [Frozen] Mock<IMediator> mediator,
        Mock<IUrlActionHelper> urlActionHelper,
        [NoAutoProperties] EmployerAccountController controller)
    {
        // Arrange
        SetControllerContextUserIdClaim(userId, controller);
        employerAccountByIdResponse.Account = new Account { Name = "some account name"};

        encodingService.Setup(e => e.Decode(hashedAccountId, EncodingType.AccountId)).Returns(accountId);
        
        mediator
            .Setup(m => m.Send(
                It.Is<GetEmployerAccountByIdQuery>(q => q.UserId == userId && q.AccountId == accountId), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(employerAccountByIdResponse)
            .Verifiable();
        
        //Act
        var result = (await controller.AddTrainingProviderTriage(hashedAccountId, 2, urlActionHelper.Object)) as RedirectToRouteResult;

        //Assert
        mediator.Verify(m => m.Send(
            It.Is<GetEmployerAccountByIdQuery>(q => q.UserId == userId && q.AccountId == accountId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Test, MoqAutoData]
    public async Task ThenAccountCreateSuccessEmailIsSent(
        long accountId,
        string userId,
        string hashedAccountId,
        string publicHashedAccountId,
        Uri providersUri,
        [NoAutoProperties] GetEmployerAccountByIdResponse employerAccountByIdResponse,
        [Frozen] Mock<IEncodingService> encodingService,
        [Frozen] Mock<IMediator> mediator,
        Mock<IUrlActionHelper> urlActionHelper,
        [NoAutoProperties] EmployerAccountController controller)
    {
        // Arrange
        SetControllerContextUserIdClaim(userId, controller);
        employerAccountByIdResponse.Account = new Account { Name = "some account name"};

        encodingService.Setup(e => e.Decode(hashedAccountId, EncodingType.AccountId)).Returns(accountId);
        encodingService.Setup(e => e.Encode(accountId, EncodingType.PublicAccountId)).Returns(publicHashedAccountId);

        mediator
            .Setup(m => m.Send(
                It.Is<GetEmployerAccountByIdQuery>(q => q.UserId == userId && q.AccountId == accountId), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(employerAccountByIdResponse)
            .Verifiable();

        SendAccountTaskListCompleteNotificationCommand sentCommand = null;
        mediator
            .Setup(m => m.Send(
                It.IsAny<SendAccountTaskListCompleteNotificationCommand>(),
                It.IsAny<CancellationToken>()))
            .Callback((SendAccountTaskListCompleteNotificationCommand command, CancellationToken ctx) =>
            {
                sentCommand = command;
            });
        
        //Act
        var result = (await controller.AddTrainingProviderTriage(hashedAccountId, 2, urlActionHelper.Object)) as RedirectToRouteResult;

        //Assert
        sentCommand.Should().NotBeNull();
        sentCommand.AccountId.Should().Be(accountId);
        sentCommand.PublicHashedAccountId.Should().Be(publicHashedAccountId);
        sentCommand.HashedAccountId.Should().Be(hashedAccountId);
        sentCommand.ExternalUserId.Should().Be(userId);
        sentCommand.OrganisationName.Should().Be(employerAccountByIdResponse.Account.Name);
    }
    
    [Test, MoqAutoData]
    public async Task ThenIShouldGoToAccountCreateSuccess(
        long accountId,
        string userId,
        string hashedAccountId,
        Uri providersUri,
        [NoAutoProperties] GetEmployerAccountByIdResponse employerAccountByIdResponse,
        [Frozen] Mock<IEncodingService> encodingService,
        [Frozen] Mock<IMediator> mediator,
        Mock<IUrlActionHelper> urlActionHelper,
        [NoAutoProperties] EmployerAccountController controller)
    {
        // Arrange
        SetControllerContextUserIdClaim(userId, controller);
        employerAccountByIdResponse.Account = new Account { Name = "some account name"};

        encodingService.Setup(e => e.Decode(hashedAccountId, EncodingType.AccountId)).Returns(accountId);
        
        mediator
            .Setup(m => m.Send(
                It.Is<GetEmployerAccountByIdQuery>(q => q.UserId == userId && q.AccountId == accountId), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(employerAccountByIdResponse);
        
        //Act
        var result = (await controller.AddTrainingProviderTriage(hashedAccountId, 2, urlActionHelper.Object)) as RedirectToRouteResult;

        //Assert
        result.RouteName.Should().Be(RouteNames.CreateAccountSuccess);
    }
}