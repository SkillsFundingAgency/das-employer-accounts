namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.EmployerAccountControllerTests.PayBillTriage;

class WhenIDoNotSelectAnOption
{
    [Test, MoqAutoData]
    public void ThenIShouldReceiveAnError([NoAutoProperties] EmployerAccountController controller)
    {
        //Act
        var result = controller.PayBillTriage((int?)null) as ViewResult;

        //Assert
        result.Model?.GetType().GetProperty("InError").Should().NotBeNull();
    }
}