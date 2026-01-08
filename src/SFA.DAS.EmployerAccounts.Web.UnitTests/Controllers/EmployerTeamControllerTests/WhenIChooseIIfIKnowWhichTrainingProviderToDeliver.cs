using SFA.DAS.EmployerAccounts.Web.RouteValues;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerTeamControllerTests;

public class WhenIChooseIIfIKnowWhichTrainingProviderToDeliver
{
    [Test, MoqAutoData]
    public void IfIChooseYesIContinueTheJourney(
        string hashedAccountId,
        [NoAutoProperties] EmployerTeamController controller)
    {
        //Act
        var result = controller.TriageHaveYouChosenATrainingProvider(hashedAccountId, new TriageViewModel { TriageOption = TriageOptions.Yes }) as RedirectToRouteResult;

        //Assert
        result.RouteName.Should().Be(RouteNames.TriageWhenWillApprenticeshipStart);
    }

    [Test, MoqAutoData]
    public void IfIChooseNoICannotSetupAnApprentice(
        string hashedAccountId,
        [NoAutoProperties] EmployerTeamController controller)
    {
        //Act
        var result = controller.TriageHaveYouChosenATrainingProvider(hashedAccountId, new TriageViewModel { TriageOption = TriageOptions.No }) as RedirectToRouteResult;

        //Assert
        result.RouteName.Should().Be(RouteNames.TriageCannotSetupWithoutChosenProvider);
    }
}