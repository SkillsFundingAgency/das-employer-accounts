using Microsoft.AspNetCore.Authorization;

namespace SFA.DAS.EmployerAccounts.Web.Controllers;

[Route("agreements")]
[AllowAnonymous]
public class AnonymousEmployerAgreementController : Controller
{
    private const string ViewPath = "~/Views/EmployerAgreement/Unsigned.cshtml";

    [HttpGet]
    [Route("")]
    public IActionResult Index(string returnUrl, string legalEntityName)
    {
        var model = new SignEmployerAgreementViewModel()
        {
            Choice = default,
            PreviouslySignedEmployerAgreement = default,
            EmployerAgreement = new EmployerAgreementView()
            {
                Status = EmployerAgreementStatus.Signed,
                LegalEntityName = legalEntityName.ToUpper(),
                TemplateId = default,
                TemplatePartialViewName = "_Agreement_v9",
                //SignedByName = "Deven",
                //LegalEntityAddress = "30 cranborne avenue, wescroft, milton keynes mk44et",
                //SignedDate = DateTime.Now,
            },
            //HasAcknowledgedAgreement = default,
        };
        return View(ViewPath, model);
    }
}
