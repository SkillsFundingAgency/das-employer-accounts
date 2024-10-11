using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.EmployerAccounts.Infrastructure;
using SFA.DAS.EmployerAccounts.Web.Authentication;
using SFA.DAS.EmployerAccounts.Web.RouteValues;
using SFA.DAS.EmployerUsers.WebClientComponents;
using SFA.DAS.GovUK.Auth.Models;
using SFA.DAS.GovUK.Auth.Services;

namespace SFA.DAS.EmployerAccounts.Web.Controllers;

[Route("service")]
public class HomeController : BaseController
{
    private readonly IHomeOrchestrator _homeOrchestrator;
    private readonly EmployerAccountsConfiguration _configuration;
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _config;
    private readonly IStubAuthenticationService _stubAuthenticationService;
    private readonly IUrlActionHelper _urlHelper;

    public HomeController(
        IHomeOrchestrator homeOrchestrator,
        EmployerAccountsConfiguration configuration,
        ICookieStorageService<FlashMessageViewModel> flashMessage,
        ILogger<HomeController> logger,
        IConfiguration config,
        IStubAuthenticationService stubAuthenticationService, IUrlActionHelper urlHelper)
        : base(flashMessage)
    {
        _homeOrchestrator = homeOrchestrator;
        _configuration = configuration;
        _logger = logger;
        _config = config;
        _stubAuthenticationService = stubAuthenticationService;
        _urlHelper = urlHelper;
    }

    [Route("~/")]
    [Route("Index")]
    public async Task<IActionResult> Index(
        GaQueryData gaQueryData,
        [FromQuery(Name = "redirectUri")] string redirectUri)
    {
        if (User.Identities.FirstOrDefault() != null && User.Identities.FirstOrDefault()!.IsAuthenticated)
        {
            var userRef = HttpContext.User.FindFirstValue(EmployerClaims.IdamsUserIdClaimTypeIdentifier);

            var userDetail = await _homeOrchestrator.GetUser(userRef);

            if (userDetail == null || string.IsNullOrEmpty(userDetail.FirstName) || string.IsNullOrEmpty(userDetail.LastName) || string.IsNullOrEmpty(userRef))
            {
                return Redirect(_urlHelper.EmployerProfileAddUserDetails($"/user/add-user-details") + $"?_ga={gaQueryData._ga}&_gl={gaQueryData._gl}&utm_source={gaQueryData.utm_source}&utm_campaign={gaQueryData.utm_campaign}&utm_medium={gaQueryData.utm_medium}&utm_keywords={gaQueryData.utm_keywords}&utm_content={gaQueryData.utm_content}");
            }
        }

        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(x => x.Type.Equals(ControllerConstants.UserRefClaimKeyName));

        OrchestratorResponse<UserAccountsViewModel> accounts;

        if (userIdClaim != null)
        {
            await _homeOrchestrator.RecordUserLoggedIn(userIdClaim.Value);

            accounts = await _homeOrchestrator.GetUserAccounts(
                userIdClaim.Value,
                gaQueryData,
                redirectUri,
                _configuration.ValidRedirectUris,
                _configuration.LastTermsAndConditionsUpdate);
        }
        else
        {
            if (_config["ResourceEnvironmentName"].Equals("prd"))
            {
                //GDS requirement that users begin their service journey on .gov.uk
                return Redirect(_configuration.GovUkSignInToASAccountUrl);
            }

            var model = new ServiceStartPageViewModel
            {
                HideHeaderSignInLink = true,
            };

            return View(ControllerConstants.ServiceStartPageViewName, model);
        }

        if (accounts.Data.Invitations > 0)
        {
            return RedirectToAction(ControllerConstants.InvitationIndexName, ControllerConstants.InvitationControllerName, gaQueryData);
        }

        // condition to check if the user has only one account, then redirect to the given
        // redirectUri if valid or redirect to home page/dashboard.
        if (accounts.Data.Accounts.AccountList.Count == 1)
        {
            var account = accounts.Data.Accounts.AccountList.FirstOrDefault();

            if (account != null)
            {
                if (account.AddTrainingProviderAcknowledged ?? true)
                {
                    // the redirectUri is validated against configuration during model setup
                    var redirectUriWithHashedAccountId = accounts.Data.RedirectUriWithHashedAccountId(account);
                    if (!string.IsNullOrEmpty(redirectUriWithHashedAccountId))
                    {
                        _logger.LogInformation($"Redirecting to {redirectUriWithHashedAccountId}");
                        return Redirect(redirectUriWithHashedAccountId);
                    }

                    _logger.LogInformation($"Redirecting to {RouteNames.EmployerTeamIndex}");
                    return RedirectToRoute(RouteNames.EmployerTeamIndex, new
                    {
                        HashedAccountId = account.HashedId,
                        gaQueryData._ga,
                        gaQueryData._gl,
                        gaQueryData.utm_source,
                        gaQueryData.utm_campaign,
                        gaQueryData.utm_medium,
                        gaQueryData.utm_keywords,
                        gaQueryData.utm_content
                    });
                }
                else
                {
                    _logger.LogInformation($"Redirecting to {RouteNames.ContinueNewEmployerAccountTaskList}");
                    return RedirectToRoute(RouteNames.ContinueNewEmployerAccountTaskList, new { hashedAccountId = account.HashedId });
                }
            }
        }

        var flashMessage = GetFlashMessageViewModelFromCookie();

        if (flashMessage != null)
        {
            accounts.FlashMessage = flashMessage;
        }

        // condition to check if the user has more than one account, then show the accounts page.
        if (accounts.Data.Accounts.AccountList.Count > 1)
        {
            return View(accounts);
        }

        _logger.LogInformation($"Redirecting to {RouteNames.NewEmployerAccountTaskList}");
        return RedirectToRoute(RouteNames.NewEmployerAccountTaskList, gaQueryData);
    }

    [Authorize]
    public IActionResult GovSignIn(
        GaQueryData gaQueryData,
        [FromQuery(Name = "redirectUri")] string redirectUri)
    {
        return RedirectToAction(ControllerConstants.IndexActionName, "Home", new { gaQueryData, redirectUri });
    }

    [HttpGet]
    [Route("termsAndConditions/overview")]
    public IActionResult TermsAndConditionsOverview()
    {
        return View();
    }

    [HttpGet]
    [Authorize(Policy = nameof(PolicyNames.HasEmployerViewerTransactorOwnerAccount))]
    [Route("termsAndConditions")]
    public IActionResult TermsAndConditions(string returnUrl, string hashedAccountId)
    {
        var termsAndConditionsNewViewModel = new TermsAndConditionsNewViewModel { ReturnUrl = returnUrl, HashedAccountId = hashedAccountId };
        return View(termsAndConditionsNewViewModel);
    }

    [HttpPost]
    [Authorize(Policy = nameof(PolicyNames.HasEmployerViewerTransactorOwnerAccount))]
    [Route("termsAndConditions")]
    public async Task<IActionResult> TermsAndConditions(TermsAndConditionsNewViewModel termsAndConditionViewModel)
    {
        var userRef = HttpContext.User.FindFirstValue(ControllerConstants.UserRefClaimKeyName);
        await _homeOrchestrator.UpdateTermAndConditionsAcceptedOn(userRef);

        if (termsAndConditionViewModel.ReturnUrl == "EmployerTeam")
        {
            return RedirectToAction(ControllerConstants.IndexActionName, ControllerConstants.EmployerTeamControllerName, new { termsAndConditionViewModel.HashedAccountId });
        }

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = nameof(PolicyNames.HasEmployerViewerTransactorOwnerAccount))]
    [HttpGet]
    [Route("accounts", Name = RouteNames.AccountsIndex)]
    public async Task<IActionResult> ViewAccounts()
    {
        var accounts = await _homeOrchestrator.GetUserAccounts(HttpContext.User.FindFirstValue(ControllerConstants.UserRefClaimKeyName));
        return View(ControllerConstants.IndexActionName, accounts);
    }

    [Authorize(Policy = nameof(PolicyNames.HasEmployerViewerTransactorOwnerAccount))]
    [HttpGet]
    [Route("register/new/{correlationId?}")]
    public IActionResult HandleNewRegistration(string correlationId = null)
    {
        return RedirectToAction(ControllerConstants.IndexActionName);
    }

    [HttpGet]
    [Route("register")]
    [Route("register/{correlationId}")]
    public async Task<IActionResult> RegisterUser(Guid? correlationId)
    {
        if (!correlationId.HasValue)
        {
            return Redirect(_urlHelper.EmployerProfileAddUserDetails($"/user/add-user-details"));
        }

        var invitation = await _homeOrchestrator.GetProviderInvitation(correlationId.Value);

        var queryData = invitation.Data != null
            ? $"?correlationId={correlationId}&firstname={WebUtility.UrlEncode(invitation.Data.EmployerFirstName)}&lastname={WebUtility.UrlEncode(invitation.Data.EmployerLastName)}"
            : "";
        return Redirect(_urlHelper.EmployerProfileAddUserDetails($"/user/add-user-details") + queryData);
    }

    [Authorize(Policy = nameof(PolicyNames.HasEmployerViewerTransactorOwnerAccount))]
    [HttpGet]
    [Route("password/change")]
    public IActionResult HandlePasswordChanged(bool userCancelled = false)
    {
        if (!userCancelled)
        {
            var flashMessage = new FlashMessageViewModel
            {
                Severity = FlashMessageSeverityLevel.Success,
                Headline = "You've changed your password"
            };
            AddFlashMessageToCookie(flashMessage);
        }

        return RedirectToAction(ControllerConstants.IndexActionName);
    }

    [Authorize(Policy = nameof(PolicyNames.HasEmployerViewerTransactorOwnerAccount))]
    [HttpGet]
    [Route("email/change")]
    public async Task<IActionResult> HandleEmailChanged(bool userCancelled = false)
    {
        if (!userCancelled)
        {
            var flashMessage = new FlashMessageViewModel
            {
                Severity = FlashMessageSeverityLevel.Success,
                Headline = "You've changed your email"
            };

            AddFlashMessageToCookie(flashMessage);

            var userRef = HttpContext.User.FindFirstValue(ControllerConstants.UserRefClaimKeyName);
            var email = HttpContext.User.FindFirstValue(EmployerClaims.IdamsUserEmailClaimTypeIdentifier);
            var firstName = HttpContext.User.FindFirstValue(DasClaimTypes.GivenName);
            var lastName = HttpContext.User.FindFirstValue(DasClaimTypes.FamilyName);

            await _homeOrchestrator.SaveUpdatedIdentityAttributes(userRef, email, firstName, lastName);
        }

        return RedirectToAction(ControllerConstants.IndexActionName);
    }

    [Authorize]
    [Route("signIn")]
    public IActionResult SignIn()
    {
        return RedirectToAction(ControllerConstants.IndexActionName);
    }

    [Route("signOut", Name = RouteNames.SignOut)]
    public async Task<IActionResult> SignOutUser()
    {
        var idToken = await HttpContext.GetTokenAsync("id_token");

        var authenticationProperties = new AuthenticationProperties();
        authenticationProperties.Parameters.Clear();
        authenticationProperties.Parameters.Add("id_token", idToken);
        var schemes = new List<string>
            {
                CookieAuthenticationDefaults.AuthenticationScheme
            };
        _ = bool.TryParse(_config["StubAuth"], out var stubAuth);
        if (!stubAuth)
        {
            schemes.Add(OpenIdConnectDefaults.AuthenticationScheme);
        }

        return SignOut(authenticationProperties, schemes.ToArray());
    }

    [Route("signoutcleanup")]
    public async Task SignOutCleanup()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    [HttpGet]
    [Route("{HashedAccountId}/privacy", Order = 0)]
    [Route("privacy", Order = 1)]
    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    [Route("help")]
    public IActionResult Help()
    {
        return RedirectPermanent(_configuration.ZenDeskHelpCentreUrl);
    }

    [HttpGet]
    [Route("start")]
    public IActionResult ServiceStartPage()
    {
        var model = new ServiceStartPageViewModel
        {
            HideHeaderSignInLink = true,
        };

        return View(model);
    }

    [HttpGet]
    [Route("unsubscribe/{correlationId}")]
    public IActionResult Unsubscribe()
    {
        return View();
    }

    [HttpPost]
    [Route("unsubscribe/{correlationId}")]
    public async Task<IActionResult> Unsubscribe(bool? unsubscribe, string correlationId)
    {
        if (unsubscribe == null || unsubscribe == false)
        {
            var model = new
            {
                InError = true
            };

            return View(model);
        }

        await _homeOrchestrator.Unsubscribe(Guid.Parse(correlationId));

        return View(ControllerConstants.UnsubscribedViewName);
    }

#if DEBUG
    [Route("CreateLegalAgreement/{showSubFields}")]
    public IActionResult ShowLegalAgreement(bool showSubFields) //call this  with false
    {
        return View(ControllerConstants.LegalAgreementViewName, showSubFields);
    }

    [HttpGet]
    [Route("SignIn-Stub")]
    public IActionResult SigninStub(string returnUrl)
    {
        var model = new SignInStubViewModel
        {
            StubId = _config["StubId"],
            StubEmail = _config["StubEmail"],
            ReturnUrl = returnUrl
        };

        return View("SigninStub", model);
    }

    [HttpPost]
    [Route("SignIn-Stub")]
    public async Task<IActionResult> SigninStubPost(SignInStubViewModel model)
    {
        var claims = await _stubAuthenticationService.GetStubSignInClaims(new StubAuthUserDetails
        {
            Email = model.StubEmail,
            Id = model.StubId
        });

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claims,
            new AuthenticationProperties());

        return RedirectToRoute("Signed-in-stub", new { model.ReturnUrl });
    }

    [Authorize]
    [HttpGet]
    [Route("signed-in-stub", Name = "Signed-in-stub")]
    public IActionResult SignedInStub(string returnUrl)
    {
        return View(new SignedInStubViewModel(HttpContext, returnUrl));
    }
#endif
}