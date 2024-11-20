﻿using System.Threading;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAgreementTemplates;

namespace SFA.DAS.EmployerAccounts.Web.Controllers;

[Route("agreements/preview")]
[Authorize]
public class PreviewEmployerAgreementController(IMediator _mediator) : Controller
{
    public const string AgreementPreviewViewPath = "~/Views/EmployerAgreement/Preview.cshtml";

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] string legalEntityName, [FromQuery] string returnUrl, CancellationToken cancellationToken)
    {
        GetEmployerAgreementTemplatesResponse response = await _mediator.Send(new GetEmployerAgreementTemplatesRequest(), cancellationToken);
        var latestTemplate = response.EmployerAgreementTemplates.OrderByDescending(t => t.VersionNumber).First();

        var model = new PreviewEmployerAgreementViewModel()
        {
            ReturnUrl = HttpUtility.UrlDecode(returnUrl),
            Choice = default,
            PreviouslySignedEmployerAgreement = default,
            HasAcknowledgedAgreement = false,
            EmployerAgreement = new EmployerAgreementView()
            {
                LegalEntityName = legalEntityName.ToUpper(),
                TemplateId = default,
                TemplatePartialViewName = latestTemplate.PartialViewName,
            },
        };
        return View(AgreementPreviewViewPath, model);
    }
}