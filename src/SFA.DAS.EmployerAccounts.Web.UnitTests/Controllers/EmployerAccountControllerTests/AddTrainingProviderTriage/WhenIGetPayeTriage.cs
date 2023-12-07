using AutoFixture.NUnit3;
using FluentAssertions;
using SFA.DAS.EmployerAccounts.Web.RouteValues;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerAccountControllerTests.AddTrainingProviderTriage;

[TestFixture]
class WhenIGetAddTrainingProviderTriage
{
    [Test, MoqAutoData]
    public void Then_Should_Return_Triage([NoAutoProperties] EmployerAccountController controller)
    {
        //Act
        var result = controller.PayBillTriage(string.Empty) as ViewResult;

        //Assert
        result.ViewName.Should().BeNull();
    }
}