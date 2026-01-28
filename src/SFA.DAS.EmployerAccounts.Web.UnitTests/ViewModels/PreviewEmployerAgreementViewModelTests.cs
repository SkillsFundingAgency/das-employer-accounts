namespace SFA.DAS.EmployerAccounts.Web.UnitTests.ViewModels;
public class PreviewEmployerAgreementViewModelTests
{
    [Test]
    public void WhenInstantiatingClassSetsPreviewToTrue()
    {
        PreviewEmployerAgreementViewModel sut = new()
        { ReturnUrl = string.Empty };

        sut.IsPreviewingAgreement.Should().BeTrue();
    }
}
