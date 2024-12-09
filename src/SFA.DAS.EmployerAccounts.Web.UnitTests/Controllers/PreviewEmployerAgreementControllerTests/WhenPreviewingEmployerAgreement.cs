using System.Web;
using FluentAssertions;
using MediatR;
using SFA.DAS.EmployerAccounts.Infrastructure.DataProtection;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAgreementTemplates;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers.PreviewEmployerAgreementControllerTests;
public class WhenPreviewingEmployerAgreement
{
    private const string ExpectedPartialName = "Two";
    private const string ExpectedReturnUrl = "https://www.google.com";
    private const string ExpectedEmployerName = "Employer name";
    private const string EncryptedName = "Encrypted employer name";
    private ViewResult actualResult = null!;

    [SetUp]
    public async Task SetUp()
    {
        Mock<IDataProtectorService> protectorServiceMock = new();
        protectorServiceMock.Setup(p => p.Unprotect(EncryptedName)).Returns(ExpectedEmployerName);
        Mock<IDataProtectorServiceFactory> factoryMock = new();
        factoryMock.Setup(f => f.Create(DataProtectionKeys.EmployerName)).Returns(protectorServiceMock.Object);

        Mock<IMediator> mediatorMock = new();
        GetEmployerAgreementTemplatesResponse response = new()
        {
            EmployerAgreementTemplates =
            [
                new () { VersionNumber = 1, PartialViewName = "One" },
                new () { VersionNumber = 2, PartialViewName = ExpectedPartialName }
            ]
        };
        mediatorMock.Setup(m => m.Send(It.IsAny<GetEmployerAgreementTemplatesRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);

        PreviewEmployerAgreementController sut = new(mediatorMock.Object, factoryMock.Object);

        actualResult = (ViewResult)(await sut.Index(EncryptedName, HttpUtility.UrlEncode(ExpectedReturnUrl), CancellationToken.None));
    }

    [Test]
    public void ThenEmployerAgreementPreviewViewIsReturned()
    {
        actualResult.ViewName.Should().Be(PreviewEmployerAgreementController.AgreementPreviewViewPath);
    }

    [Test]
    public void ThenModeIsOfTypePreviewEmployerAgreementViewModel()
    {
        actualResult.Model.Should().BeOfType<PreviewEmployerAgreementViewModel>();
    }

    [Test]
    public void ThenViewModelHasPreviewSetToTrue()
    {
        actualResult.Model.As<PreviewEmployerAgreementViewModel>().IsPreviewingAgreement.Should().BeTrue();
    }

    [Test]
    public void ThenViewModelHasExpectedReturnUrl()
    {
        actualResult.Model.As<PreviewEmployerAgreementViewModel>().ReturnUrl.Should().Be(ExpectedReturnUrl);
    }

    [Test]
    public void ThenViewModelHasExpectedEmployerName()
    {
        actualResult.Model.As<PreviewEmployerAgreementViewModel>().EmployerAgreement.LegalEntityName.Should().Be(ExpectedEmployerName.ToUpper());
    }

    [Test]
    public void ThenViewModelHasExpectedPartialName()
    {
        actualResult.Model.As<PreviewEmployerAgreementViewModel>().EmployerAgreement.TemplatePartialViewName.Should().Be(ExpectedPartialName);
    }
}
