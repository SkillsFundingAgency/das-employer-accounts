namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerAccountControllerTests.AddTrainingProviderTriage;

class WhenIDoNotSelectAnOption : EmployerAccountControllerTestsBase
{
    [Test, MoqAutoData]
    public async Task ThenIShouldReceiveAnError(
        string userId,
        string hashedAccountId,
        Mock<IUrlActionHelper> urlActionHelper,
        [NoAutoProperties] EmployerAccountController controller)
    {
        // Arrange 
        SetControllerContextUserIdClaim(userId, controller);
        
        //Act
        var result = (await controller.AddTrainingProviderTriage(hashedAccountId, null, urlActionHelper.Object)) as ViewResult;

        //Assert
        result?.Model?.GetType().GetProperty("InError").Should().NotBeNull();
    }
}