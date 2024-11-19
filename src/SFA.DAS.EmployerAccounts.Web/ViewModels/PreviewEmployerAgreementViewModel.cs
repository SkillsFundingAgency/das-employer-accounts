namespace SFA.DAS.EmployerAccounts.Web.ViewModels;

public class PreviewEmployerAgreementViewModel : SignEmployerAgreementViewModel
{
    public PreviewEmployerAgreementViewModel()
    {
        IsPreviewingAgreement = true;
    }

    public required string ReturnUrl { get; set; }
}
