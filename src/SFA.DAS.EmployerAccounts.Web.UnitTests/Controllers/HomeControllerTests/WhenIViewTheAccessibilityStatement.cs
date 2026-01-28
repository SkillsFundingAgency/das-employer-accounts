namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.HomeControllerTests;

public class WhenIViewTheAccessibilityStatement
{
    [Test, MoqAutoData]
    public void Then_AccessibilityStatement_View_Is_Returned([Greedy] HomeController controller)
    {
        var result = controller.AccessibilityStatement() as ViewResult;

        result.Should().NotBeNull();
    }

}