using AutoFixture.NUnit3;
using FluentAssertions;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerAccountControllerTests.AddTrainingProviderTriage;

class WhenIChooseToAddATrainingProvider : EmployerAccountControllerTestsBase
{
    [Test, MoqAutoData]
    public async Task ThenIShouldGoToGatewayInform(   
        string userId,
        string hashedAccountId,
        Uri providersUri,
        Mock<IUrlActionHelper> urlActionHelper,
        [NoAutoProperties] EmployerAccountController controller)
    {
        // Arrange
        var expectedRedirect = $"{providersUri.AbsoluteUri}?AccountTasks=true";
        SetControllerContextUserIdClaim(userId, controller);
        urlActionHelper
            .Setup(u => u.ProviderRelationshipsAction(It.Is<string>(s =>
                s.Equals("providers", StringComparison.InvariantCultureIgnoreCase))))
            .Returns(providersUri.AbsoluteUri);
        
        //Act
        var result = (await controller.AddTrainingProviderTriage(hashedAccountId, 1, urlActionHelper.Object)) as RedirectResult;

        //Assert
        result.Url.Should().Be(expectedRedirect);
    }
}