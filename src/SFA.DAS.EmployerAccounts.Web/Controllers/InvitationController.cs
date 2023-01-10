﻿using System.Linq;
using SFA.DAS.Authorization.Mvc.Attributes;
using SFA.DAS.EmployerAccounts.Configuration;

namespace SFA.DAS.EmployerAccounts.Web.Controllers;

[Route("invitations")]
public class InvitationController : BaseController
{
    private readonly InvitationOrchestrator _invitationOrchestrator;

    private readonly EmployerAccountsConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public InvitationController(InvitationOrchestrator invitationOrchestrator, IAuthenticationService owinWrapper, 
        IMultiVariantTestingService multiVariantTestingService,
        EmployerAccountsConfiguration configuration,
        ICookieStorageService<FlashMessageViewModel> flashMessage,
        IHttpContextAccessor httpContextAccessor)
        : base(owinWrapper, multiVariantTestingService, flashMessage)
    {
        _invitationOrchestrator = invitationOrchestrator ?? throw new ArgumentNullException(nameof(invitationOrchestrator));
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    [Route("invite")]
    public IActionResult Invite()
    {
        if (string.IsNullOrEmpty(OwinWrapper.GetClaimValue(ControllerConstants.UserRefClaimKeyName)))
        {
            return View();
        }

        return RedirectToAction(ControllerConstants.IndexActionName, ControllerConstants.HomeControllerName);
    }

    [HttpGet]
    [DasAuthorize]
    public async Task<IActionResult> All()
    {
        if (string.IsNullOrEmpty(OwinWrapper.GetClaimValue(ControllerConstants.UserRefClaimKeyName)))
        {
            return RedirectToAction(ControllerConstants.IndexActionName, ControllerConstants.HomeControllerName);
        }

        var model = await _invitationOrchestrator.GetAllInvitationsForUser(OwinWrapper.GetClaimValue("sub"));

        return View(model);
    }

    [HttpGet]
    [DasAuthorize]
    [Route("view")]
    public async Task<IActionResult> Details(string invitationId)
    {
        if (string.IsNullOrEmpty(OwinWrapper.GetClaimValue(ControllerConstants.UserRefClaimKeyName)))
        {
            return RedirectToAction(ControllerConstants.IndexActionName, ControllerConstants.HomeControllerName);
        }

        var invitation = await _invitationOrchestrator.GetInvitation(invitationId);

        return View(invitation);
    }

    [HttpPost]
    [DasAuthorize]
    [ValidateAntiForgeryToken]
    [Route("accept")]
    public async Task<IActionResult> Accept(long invitation, UserInvitationsViewModel model)
    {
        if (string.IsNullOrEmpty(OwinWrapper.GetClaimValue(ControllerConstants.UserRefClaimKeyName)))
        {
            return RedirectToAction(ControllerConstants.IndexActionName, ControllerConstants.HomeControllerName);
        }

        var invitationItem = model.Invitations.SingleOrDefault(c => c.Id == invitation);

        if (invitationItem == null)
        {
            return RedirectToAction(ControllerConstants.IndexActionName, ControllerConstants.HomeControllerName);
        }

        await _invitationOrchestrator.AcceptInvitation(invitationItem.Id, OwinWrapper.GetClaimValue(ControllerConstants.UserRefClaimKeyName));

        var flashMessage = new FlashMessageViewModel
        {
            Headline = "Invitation accepted",
            Message = $"You can now access the {invitationItem.AccountName} account",
            Severity = FlashMessageSeverityLevel.Success
        };
        AddFlashMessageToCookie(flashMessage);


        return RedirectToAction(ControllerConstants.IndexActionName, ControllerConstants.HomeControllerName);
    }

    [HttpPost]
    [DasAuthorize]
    [ValidateAntiForgeryToken]
    [Route("create")]
    public async Task<IActionResult> Create(InviteTeamMemberViewModel model)
    {
        if (string.IsNullOrEmpty(OwinWrapper.GetClaimValue(ControllerConstants.UserRefClaimKeyName)))
        {
            return RedirectToAction(ControllerConstants.IndexActionName, ControllerConstants.HomeControllerName);
        }

        await _invitationOrchestrator.CreateInvitation(model, OwinWrapper.GetClaimValue(ControllerConstants.UserRefClaimKeyName));

        return RedirectToAction(ControllerConstants.IndexActionName, ControllerConstants.HomeControllerName);
    }

    [HttpGet]
    [Route("register-and-accept")]
    public IActionResult AcceptInvitationNewUser()
    {
        var schema = _httpContextAccessor.HttpContext?.Request.Scheme;
        var authority = _httpContextAccessor.HttpContext?.Request.Host;
        var appConstants = new Constants(_configuration.Identity);
        return new RedirectResult($"{appConstants.RegisterLink()}{schema}://{authority}/invitations");
    }


    [HttpGet]
    [Route("accept")]
    public IActionResult AcceptInvitationExistingUser()
    {
        return RedirectToAction("All");
    }
}