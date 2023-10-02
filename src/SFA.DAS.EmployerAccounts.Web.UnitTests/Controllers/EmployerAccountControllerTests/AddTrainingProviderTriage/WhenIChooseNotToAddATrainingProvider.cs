using AutoFixture.NUnit3;
using FluentAssertions;
using SFA.DAS.EmployerAccounts.Web.RouteValues;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerAccountControllerTests.AddTrainingProviderTriage;

class WhenIChooseNotToAddATrainingProvider : EmployerAccountControllerTestsBase
{
    [Test, MoqAutoData]
    public async Task ThenIShouldGoToAccountCreateSuccess(
        string userId,
        string hashedAccountId,
        Mock<IUrlActionHelper> urlActionHelper,
        [NoAutoProperties] EmployerAccountController controller)
    {
        // Arrange
        SetControllerContextUserIdClaim(userId, controller);
        
        //Act
        var result = (await controller.AddTrainingProviderTriage(hashedAccountId, 2, urlActionHelper.Object)) as RedirectToRouteResult;

        //Assert
        result.RouteName.Should().Be(RouteNames.CreateAccountSuccess);
    }
}