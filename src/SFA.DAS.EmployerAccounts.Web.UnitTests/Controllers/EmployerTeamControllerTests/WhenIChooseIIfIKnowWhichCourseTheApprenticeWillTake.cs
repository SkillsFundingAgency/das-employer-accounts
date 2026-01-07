using SFA.DAS.EmployerAccounts.Web.RouteValues;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerTeamControllerTests;

public class WhenIChooseIIfIKnowWhichCourseTheApprenticeWillTake
{
    [Test, MoqAutoData]
    public void IfIChooseYesIContinueTheJourney(
        string hashedAccountId,
        [NoAutoProperties] EmployerTeamController controller)
    {
        //Act
        var result = controller.TriageWhichCourseYourApprenticeWillTake(hashedAccountId, new TriageViewModel { TriageOption = TriageOptions.Yes }) as RedirectToRouteResult;

        //Assert
        result.RouteName.Should().Be(RouteNames.TriageChosenProvider);
    }

    [Test, MoqAutoData]
    public void IfIChooseNoICannotSetupAnApprentice(
        string hashedAccountId,
        [NoAutoProperties] EmployerTeamController controller)
    {
        //Act
        var result = controller.TriageWhichCourseYourApprenticeWillTake(hashedAccountId, new TriageViewModel { TriageOption = TriageOptions.No }) as RedirectToRouteResult;

        //Assert
        result.RouteName.Should().Be(RouteNames.TriageCannotSetupWithoutChosenCourseAndProvider);
    }
}