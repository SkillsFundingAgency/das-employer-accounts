﻿using System.Security.Claims;

namespace SFA.DAS.EmployerAccounts.Web.ViewComponents;

public class SupportUserBannerViewComponent : ViewComponent
{
    private readonly EmployerTeamOrchestrator _employerTeamOrchestrator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SupportUserBannerViewComponent(EmployerTeamOrchestrator employerTeamOrchestrator, IHttpContextAccessor httpContextAccessor)
    {
        _employerTeamOrchestrator = employerTeamOrchestrator;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IViewComponentResult> InvokeAsync(IAccountIdentifier model = null)
    {
        Account account = null;

        if (model != null && model.HashedAccountId != null)
        {
            var externalUserId = _httpContextAccessor.HttpContext.User.FindFirstValue(ControllerConstants.UserRefClaimKeyName);

            var response = await _employerTeamOrchestrator.GetAccountSummary(model.HashedAccountId, externalUserId);
            account = response.Status != HttpStatusCode.OK ? null : response.Data.Account;
        }

        var consoleUserType = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role) == "Tier2User" ? "Service user (T2 Support)" : "Standard user";

        return View(new SupportUserBannerViewModel
        {
            Account = account,
            ConsoleUserType = consoleUserType
        });
    }
}