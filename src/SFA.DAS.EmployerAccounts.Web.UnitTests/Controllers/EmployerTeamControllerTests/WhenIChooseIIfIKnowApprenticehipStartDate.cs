using SFA.DAS.EmployerAccounts.Web.RouteValues;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerTeamControllerTests;

public class WhenIChooseIIfIKnowApprenticehipStartDate 
{
    [Test, MoqAutoData]
    public void IfIChooseYesIContinueTheJourney(
        string hashedAccountId,
        [NoAutoProperties] EmployerTeamController controller)
    {
        //Act
        var result = controller.TriageWillApprenticeshipTrainingStart(hashedAccountId, new TriageViewModel { TriageOption = TriageOptions.Yes }) as RedirectToRouteResult;

        //Assert
        result.RouteName.Should().Be(RouteNames.TriageWhenApprenticeshipForExistingEmployee);
    }

    [Test, MoqAutoData]
    public void IfIChooseNoICannotSetupAnApprentice(
        string hashedAccountId,
        [NoAutoProperties] EmployerTeamController controller)
    {
        //Act
        var result = controller.TriageWillApprenticeshipTrainingStart(hashedAccountId, new TriageViewModel { TriageOption = TriageOptions.No }) as RedirectToRouteResult;

        //Assert
        result.RouteName.Should().Be(RouteNames.TriageCannotSetupWithoutStartDate);
    }

    [Test, MoqAutoData]
    public void IfIChooseDontKnowICannotSetupAnApprentice(
        string hashedAccountId,
        [NoAutoProperties] EmployerTeamController controller)
    {
        //Act
        var result = controller.TriageWillApprenticeshipTrainingStart(hashedAccountId, new TriageViewModel { TriageOption = TriageOptions.Unknown }) as RedirectToRouteResult;

        //Assert
        result.RouteName.Should().Be(RouteNames.TriageCannotSetupWithoutApproximateStartDate);
    }
}