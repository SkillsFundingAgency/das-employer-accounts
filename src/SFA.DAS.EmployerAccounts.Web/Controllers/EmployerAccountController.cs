using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.Employer.Shared.UI.Attributes;
using SFA.DAS.EmployerAccounts.Commands.PayeRefData;
using SFA.DAS.EmployerAccounts.Infrastructure;
using SFA.DAS.EmployerAccounts.Web.Authentication;
using SFA.DAS.EmployerAccounts.Web.RouteValues;
using SFA.DAS.EmployerAccounts.Web.Validation;

namespace SFA.DAS.EmployerAccounts.Web.Controllers;

[Route("accounts")]
[Authorize(Policy = nameof(PolicyNames.HasUserAccount))]
[SetNavigationSection(NavigationSection.AccountsHome)]
public class EmployerAccountController : BaseController
{
    private readonly EmployerAccountOrchestrator _employerAccountOrchestrator;
    private readonly ILogger<EmployerAccountController> _logger;
    private readonly IMediator _mediator;
    private readonly ICookieStorageService<HashedAccountIdModel> _accountCookieStorage;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICookieStorageService<ReturnUrlModel> _returnUrlCookieStorageService;
    private readonly string _hashedAccountIdCookieName;

    private const int Over3Million = 1;
    private const int CloseTo3Million = 2;
    private const int LessThan3Million = 3;
    private const int AddPayeGovGateway = 1;
    private const int AddPayeAorn = 2;

    private const string ReturnUrlCookieName = "SFA.DAS.EmployerAccounts.Web.Controllers.ReturnUrlCookie";

    public EmployerAccountController(EmployerAccountOrchestrator employerAccountOrchestrator,
        ILogger<EmployerAccountController> logger,
        ICookieStorageService<FlashMessageViewModel> flashMessage,
        IMediator mediatr,
        ICookieStorageService<ReturnUrlModel> returnUrlCookieStorageService,
        ICookieStorageService<HashedAccountIdModel> accountCookieStorage,
        LinkGenerator linkGenerator
    ) : base(flashMessage)
    {
        _employerAccountOrchestrator = employerAccountOrchestrator;
        _logger = logger;
        _mediator = mediatr ?? throw new ArgumentNullException(nameof(mediatr));
        _returnUrlCookieStorageService = returnUrlCookieStorageService;
        _accountCookieStorage = accountCookieStorage;
        _linkGenerator = linkGenerator;
        _hashedAccountIdCookieName = typeof(HashedAccountIdModel).FullName;
    }

    [HttpGet]
    [Authorize(Policy = nameof(PolicyNames.HasEmployerViewerTransactorOwnerAccount))]
    [Route("create/tasklist", Order = 1, Name = RouteNames.NewEmployerAccountTaskList)]
    [Route("{HashedAccountId}/tasklist", Order = 2, Name = RouteNames.ContinueNewEmployerAccountTaskList)]
    public async Task<IActionResult> CreateAccountTaskList(string hashedAccountId)
    {
        var userIdClaim = HttpContext.User.Claims.First(x => x.Type.Equals(ControllerConstants.UserRefClaimKeyName));
        var accountTaskListViewModelResponse = await _employerAccountOrchestrator.GetCreateAccountTaskList(hashedAccountId, userIdClaim.Value);

        if (accountTaskListViewModelResponse.Status == HttpStatusCode.OK
            && accountTaskListViewModelResponse.Data.TaskListComplete)
        {
            return RedirectToRoute(RouteNames.EmployerTeamIndex, new { hashedAccountId });
        }

        return View(nameof(CreateAccountTaskList), accountTaskListViewModelResponse);
    }

    [HttpGet]
    [Authorize(Policy = nameof(PolicyNames.HasEmployerViewerTransactorOwnerAccount))]
    [Route("create/progress-saved", Order = 1, Name = RouteNames.NewAccountSaveProgress)]
    [Route("{HashedAccountId}/progress-saved", Order = 2, Name = RouteNames.PartialAccountSaveProgress)]
    public IActionResult CreateAccountProgressSaved(string hashedAccountId)
    {
        return View(new CreateAccountProgressSavedViewModel { HashedAccountId = hashedAccountId });
    }

    [HttpGet]
    [Authorize(Policy = nameof(PolicyNames.HasEmployerViewerTransactorOwnerAccount))]
    [Route("{HashedAccountId}/cannotAddPaye", Name = RouteNames.AddPayeShutter)]
    public IActionResult AddPayeShutter(string hashedAccountId)
    {
        return View();
    }

    [HttpGet]
    [Authorize(Policy = nameof(PolicyNames.HasEmployerViewerTransactorOwnerAccount))]
    [Route("payBill", Name = RouteNames.EmployerAccountPayBillTriage)]
    public IActionResult PayBillTriage(string hashedAccountId)
    {
        if (!string.IsNullOrEmpty(hashedAccountId))
        {
            return RedirectToRoute(RouteNames.AddPayeShutter, new { hashedAccountId });
        }

        var model = new
        {
            HideHeaderSignInLink = true
        };

        return View(model);
    }

    [HttpPost]
    [Authorize(Policy = nameof(PolicyNames.HasEmployerViewerTransactorOwnerAccount))]
    [Route("payBill", Order = 1, Name = RouteNames.EmployerAccountPayBillTriagePost)]
    public IActionResult PayBillTriage(int? choice)
    {
        switch (choice ?? 0)
        {
            case Over3Million:
            case CloseTo3Million: return RedirectToAction(ControllerConstants.GatewayInformActionName);
            case LessThan3Million: return RedirectToRoute(RouteNames.EmployerAccountGetApprenticeshipFunding);
            default:
            {
                var model = new
                {
                    InError = true
                };

                return View(model);
            }
        }
    }

    [HttpGet]
    [Authorize(Policy = nameof(PolicyNames.HasEmployerViewerTransactorOwnerAccount))]
    [Route("{HashedAccountId}/getApprenticeshipFunding", Order = 0,
        Name = RouteNames.EmployerAccountGetApprenticeshipFundingInAccount)]
    [Route("getApprenticeshipFunding", Order = 1, Name = RouteNames.EmployerAccountGetApprenticeshipFunding)]
    public IActionResult GetApprenticeshipFunding()
    {
        PopulateViewBagWithExternalUserId();
        var model = new
        {
            HideHeaderSignInLink = true
        };

        return View(model);
    }

    [HttpPost]
    [Authorize(Policy = nameof(PolicyNames.HasEmployerViewerTransactorOwnerAccount))]
    [Route("{hashedAccountId}/getApprenticeshipFunding", Order = 0,
        Name = RouteNames.EmployerAccountPostApprenticeshipFundingInAccount)]
    [Route("getApprenticeshipFunding", Order = 1, Name = RouteNames.EmployerAccountPostApprenticeshipFunding)]
    public IActionResult GetApprenticeshipFunding(string hashedAccountId, int? choice)
    {
        switch (choice ?? 0)
        {
            case AddPayeGovGateway:
                return RedirectToAction(ControllerConstants.GatewayInformActionName,
                    ControllerConstants.EmployerAccountControllerName, new { hashedAccountId });
            case AddPayeAorn:
                return RedirectToAction(ControllerConstants.SearchUsingAornActionName,
                    ControllerConstants.SearchPensionRegulatorControllerName, new { hashedAccountId });
            default:
            {
                var model = new
                {
                    InError = true
                };

                return View(model);
            }
        }
    }

    [HttpGet]
    [Route("{hashedAccountId}/gatewayInform", Order = 0)]
    [Route("gatewayInform", Order = 1)]
    public IActionResult GatewayInform(string hashedAccountId)
    {
        if (!string.IsNullOrWhiteSpace(hashedAccountId))
        {
            _accountCookieStorage.Delete(_hashedAccountIdCookieName);

            _accountCookieStorage.Create(
                new HashedAccountIdModel { Value = hashedAccountId },
                _hashedAccountIdCookieName);
        }

        var gatewayInformViewModel = new OrchestratorResponse<GatewayInformViewModel>
        {
            Data = new GatewayInformViewModel
            {
                BreadcrumbDescription = "Back to Your User Profile",
                ConfirmUrl = _linkGenerator.GetUriByAction(HttpContext, ControllerConstants.GatewayViewName,
                    ControllerConstants.EmployerAccountControllerName),
                HashedAccountId = hashedAccountId,
                CancelRoute = string.IsNullOrEmpty(hashedAccountId)
                    ? RouteNames.NewEmployerAccountTaskList
                    : RouteNames.EmployerAccountPaye,
            }
        };

        var flashMessageViewModel = GetFlashMessageViewModelFromCookie();

        if (flashMessageViewModel != null)
        {
            gatewayInformViewModel.FlashMessage = flashMessageViewModel;
        }

        return View(gatewayInformViewModel);
    }

    [HttpGet]
    [Route("gateway")]
    public async Task<IActionResult> Gateway()
    {
        var link = _linkGenerator.GetUriByAction(
            HttpContext,
            ControllerConstants.GateWayResponseActionName,
            ControllerConstants.EmployerAccountControllerName,
            null,
            HttpContext.Request.Scheme
        );

        var url = await _employerAccountOrchestrator.GetGatewayUrl(link);

        return Redirect(url);
    }

    [Route("gatewayResponse")]
    public async Task<IActionResult> GateWayResponse()
    {
        try
        {
            _logger.LogInformation("Starting processing gateway response");

            if (string.IsNullOrEmpty(Request.GetDisplayUrl()))
            {
                return RedirectToAction(ControllerConstants.SearchPensionRegulatorActionName,
                    ControllerConstants.SearchPensionRegulatorControllerName);
            }

            var response = await _employerAccountOrchestrator.GetGatewayTokenResponse(
                Request.Query[ControllerConstants.CodeKeyName],
                Url.Action(ControllerConstants.GateWayResponseActionName,
                    ControllerConstants.EmployerAccountControllerName, null, Request.Scheme),
                Request.Query);

            if (response.Status != HttpStatusCode.OK)
            {
                _logger.LogWarning("Gateway response does not indicate success. Status = {Status}.", response.Status);
                response.Status = HttpStatusCode.OK;

                AddFlashMessageToCookie(response.FlashMessage);

                return RedirectToAction(ControllerConstants.GatewayInformActionName);
            }

            var externalUserId = HttpContext.User.FindFirstValue(EmployerClaims.IdamsUserIdClaimTypeIdentifier);
            _logger.LogInformation("Gateway response is for user identity ID {ExternalUserId}", externalUserId);

            var email = HttpContext.User.FindFirstValue(EmployerClaims.IdamsUserEmailClaimTypeIdentifier);
            var empRef =
                await _employerAccountOrchestrator.GetHmrcEmployerInformation(response.Data.AccessToken, email);
            _logger.LogInformation("Gateway response is for empRef {empRef} \n {SerializedEmpRef}", empRef.Empref,
                JsonConvert.SerializeObject(empRef));

            await _mediator.Send(new SavePayeRefData(new EmployerAccountPayeRefData
            {
                EmployerRefName = empRef.EmployerLevyInformation?.Employer?.Name?.EmprefAssociatedName ?? "",
                PayeReference = empRef.Empref,
                AccessToken = response.Data.AccessToken,
                RefreshToken = response.Data.RefreshToken,
                EmpRefNotFound = empRef.EmprefNotFound,
            }));

            _logger.LogInformation("Finished processing gateway response");

            if (string.IsNullOrEmpty(empRef.Empref) || empRef.EmprefNotFound)
            {
                return RedirectToAction(ControllerConstants.PayeErrorActionName,
                    new
                    {
                        NotFound = empRef.EmprefNotFound
                    });
            }

            return RedirectToAction(ControllerConstants.SearchPensionRegulatorActionName,
                ControllerConstants.SearchPensionRegulatorControllerName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Gateway response - {Ex}", ex);
            throw;
        }
    }

    [HttpGet]
    [Route("payeerror")]
    public ViewResult PayeError(bool? notFound)
    {
        ViewBag.NotFound = notFound ?? false;
        return View();
    }

    [HttpGet]
    [Route("summary")]
    public ViewResult Summary()
    {
        var result = _employerAccountOrchestrator.GetSummaryViewModel(HttpContext);
        return View(result);
    }

    [HttpPost]
    [Route("summary", Name = RouteNames.SummaryPost)]
    public async Task<IActionResult> Summary(SummaryViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var response = new OrchestratorResponse<SummaryViewModel>
            {
                Data = model
            };

            return View(response);
        }

        if (!model.IsOrganisationWithCorrectAddress.GetValueOrDefault())
        {
            return RedirectToRoute(RouteNames.OrganisationWrongAddress);
        }

        return await CreateAccountFromCookieData();
    }

    [HttpGet]
    [Route("organisation-wrong-address", Name = RouteNames.OrganisationWrongAddress)]
    public IActionResult OrganisationWrongAddress()
    {
        return View();
    }

    [HttpPost]
    [Route("organisation-wrong-address", Name = RouteNames.OrganisationWrongAddressPost)]
    public async Task<IActionResult> OrganisationWrongAddressPost()
    {
        return await CreateAccountFromCookieData();
    }

    [HttpGet]
    [Route("{HashedAccountId}/rename", Name = RouteNames.RenameAccount)]
    public async Task<IActionResult> RenameAccount(string hashedAccountId)
    {
        var userIdClaim = HttpContext.User.FindFirstValue(ControllerConstants.UserRefClaimKeyName);
        var vm = await _employerAccountOrchestrator.GetRenameEmployerAccountViewModel(hashedAccountId, userIdClaim);
        return View(vm);
    }

    [HttpPost]
    [Route("{HashedAccountId}/rename", Name = RouteNames.RenameAccountPost)]
    public async Task<IActionResult> RenameAccount(string hashedAccountId, RenameEmployerAccountViewModel vm)
    {
        var validator = new RenameEmployerAccountViewModelValidator(RenameAccountType.RenameAccount);
        var validationResult = validator.Validate(vm);

        if (!validationResult.IsValid)
        {
            foreach (var validationError in validationResult.Errors)
            {
                vm.ErrorDictionary.Add(validationError.PropertyName, validationError.ErrorMessage);
            }
        }

        OrchestratorResponse<RenameEmployerAccountViewModel> response;

        if (vm.ErrorDictionary.Count > 0)
        {
            response = new OrchestratorResponse<RenameEmployerAccountViewModel>
            {
                Data = vm,
                Status = HttpStatusCode.BadRequest
            };

            return View(response);
        }

        var userIdClaim = HttpContext.User.FindFirstValue(ControllerConstants.UserRefClaimKeyName);
        response = await _employerAccountOrchestrator.RenameEmployerAccount(hashedAccountId, vm, userIdClaim);

        if (response.Status == HttpStatusCode.OK)
        {
            var flashMessage = new FlashMessageViewModel
            {
                Headline = "Account renamed",
                Message = "You successfully updated the account name",
                Severity = FlashMessageSeverityLevel.Success
            };

            AddFlashMessageToCookie(flashMessage);

            return RedirectToRoute(RouteNames.EmployerTeamIndex, new { hashedAccountId });
        }

        var errorResponse = new OrchestratorResponse<RenameEmployerAccountViewModel>();

        if (response.Status == HttpStatusCode.BadRequest)
        {
            vm.ErrorDictionary = response.FlashMessage.ErrorMessages;
        }

        errorResponse.Data = vm;
        errorResponse.FlashMessage = response.FlashMessage;
        errorResponse.Status = response.Status;

        return View(errorResponse);
    }

    [HttpGet]
    [Route("{HashedAccountId}/create/accountName", Name = RouteNames.AccountName)]
    public async Task<IActionResult> AccountName(string hashedAccountId)
    {
        var vm = await GetRenameViewModel(hashedAccountId);
        return View(vm);
    }

    [HttpPost]
    [Route("{HashedAccountId}/create/accountName", Name = RouteNames.AccountNamePost)]
    public async Task<IActionResult> AccountName(string hashedAccountId, RenameEmployerAccountViewModel vm)
    {
        var response = new OrchestratorResponse<RenameEmployerAccountViewModel>();

        if (!ModelState.IsValid || !vm.ChangeAccountName.HasValue)
        {
            vm.ErrorDictionary.Add(nameof(vm.ChangeAccountName),
                "Please select whether you wish to set a new Employer Account name.");
            response.Data = vm;
            response.Status = HttpStatusCode.BadRequest;
            return View(response);
        }

        if (vm.ChangeAccountName.Value)
        {
            return HandleChangeAccountName(hashedAccountId, vm, response);
        }

        return await HandleSetEmployerAccountNameAsync(hashedAccountId, vm);
    }

    [HttpGet]
    [Route("{HashedAccountId}/create/accountName/confirm", Name = RouteNames.AccountNameConfirm)]
    public IActionResult AccountNameConfirm(string hashedAccountId, string newAccountName)
    {
        return View(new RenameEmployerAccountViewModel
        {
            ChangeAccountName = true,
            NewName = Uri.UnescapeDataString(newAccountName)
        });
    }

    [HttpPost]
    [Route("{HashedAccountId}/create/accountName/confirm", Name = RouteNames.AccountNameConfirmPost)]
    public async Task<IActionResult> AccountNameConfirm(string hashedAccountId, RenameEmployerAccountViewModel vm)
    {
        var userIdClaim = HttpContext.User.FindFirstValue(ControllerConstants.UserRefClaimKeyName);
        var response = await _employerAccountOrchestrator.SetEmployerAccountName(hashedAccountId, vm, userIdClaim);

        if (response.Status == HttpStatusCode.OK)
        {
            return RedirectToRoute(RouteNames.AccountNameSuccess, new { hashedAccountId });
        }

        response.Data = vm;

        return View(response.Data);
    }

    [HttpGet]
    [Route("{HashedAccountId}/create/orgAndPaye/success", Name = RouteNames.OrganisationAndPayeAddedSuccess)]
    public IActionResult OrganisationAndPayeAddedSuccess(string hashedAccountId)
    {
        return View();
    }

    [HttpGet]
    [Route("{HashedAccountId}/create/accountName/confirm/success", Name = RouteNames.AccountNameConfirmSuccess)]
    public IActionResult AccountNameConfirmSuccess(string hashedAccountId)
    {
        return View();
    }

    [HttpGet]
    [Route("{HashedAccountId}/create/accountName/success", Name = RouteNames.AccountNameSuccess)]
    public async Task<IActionResult> AccountNameSuccess(string hashedAccountId)
    {
        var vm = await GetRenameViewModel(hashedAccountId);
        return View(vm);
    }

    [HttpGet]
    [Route("{HashedAccountId}/create/agreement/success", Name = RouteNames.TaskListSignedAgreementSuccess)]
    public async Task<IActionResult> TaskListSignedAgreementSuccess(string hashedAccountId)
    {
        return View();
    }

    [HttpGet]
    [Route("{HashedAccountId}/create/success", Name = RouteNames.CreateAccountSuccess)]
    public async Task<IActionResult> CreateAccountSuccess(string hashedAccountId)
    {
        return View();
    }

    [HttpGet]
    [Authorize(Policy = nameof(PolicyNames.HasEmployerViewerTransactorOwnerAccount))]
    [Route("{HashedAccountId}/training-provider-triage", Name = RouteNames.TrainingProviderTriage)]
    public IActionResult AddTrainingProviderTriage()
    {
        var model = new
        {
            HideHeaderSignInLink = true
        };

        return View(model);
    }

    [HttpPost]
    [Authorize(Policy = nameof(PolicyNames.HasEmployerViewerTransactorOwnerAccount))]
    [Route("{hashedAccountId}/training-provider-triage", Name = RouteNames.TrainingProviderTriage)]
    public async Task<IActionResult> AddTrainingProviderTriage(string hashedAccountId, int? choice,
        [FromServices] IUrlActionHelper urlHelper)
    {
        var externalUserId = GetUserId();

        switch (choice ?? 0)
        {
            case 1: return Redirect(urlHelper.ProviderRelationshipsAction("providers") + $"?AccountTasks=true");
            case 2:
                await _employerAccountOrchestrator.AcknowledgeTrainingProviderTask(hashedAccountId, externalUserId);
                return RedirectToRoute(RouteNames.CreateAccountSuccess, new { hashedAccountId });
            default:
            {
                var model = new
                {
                    InError = true
                };

                return View(model);
            }
        }
    }

    [HttpGet]
    [Authorize(Policy = nameof(PolicyNames.HasEmployerViewerTransactorOwnerAccount))]
    [Route("{HashedAccountId}/training-provider-success", Name = RouteNames.TrainingProviderSuccess)]
    public IActionResult AddTrainingProviderSuccess()
    {
        var model = new
        {
            HideHeaderSignInLink = true
        };

        return View(model);
    }

    [HttpPost]
    [Authorize(Policy = nameof(PolicyNames.HasEmployerViewerTransactorOwnerAccount))]
    [Route("{hashedAccountId}/training-provider-success", Name = RouteNames.TrainingProviderSuccess)]
    public async Task<IActionResult> AddTrainingProviderTriage(string hashedAccountId)
    {
        var externalUserId = GetUserId();

        await _employerAccountOrchestrator.AcknowledgeTrainingProviderTask(hashedAccountId, externalUserId);
        return RedirectToRoute(RouteNames.CreateAccountSuccess, new { hashedAccountId });
    }

    [HttpGet]
    [Route("amendOrganisation")]
    public IActionResult AmendOrganisation()
    {
        var employerAccountData = _employerAccountOrchestrator.GetCookieData();

        if (employerAccountData.EmployerAccountOrganisationData.OrganisationType ==
            OrganisationType.PensionsRegulator && employerAccountData.EmployerAccountOrganisationData
                .PensionsRegulatorReturnedMultipleResults)
        {
            if (!string.IsNullOrWhiteSpace(employerAccountData.EmployerAccountPayeRefData.AORN))
            {
                return RedirectToAction(
                    ControllerConstants.SearchUsingAornActionName,
                    ControllerConstants.SearchPensionRegulatorControllerName,
                    new
                    {
                        Aorn = employerAccountData.EmployerAccountPayeRefData.AORN,
                        payeRef = employerAccountData.EmployerAccountPayeRefData.PayeReference
                    });
            }

            return RedirectToAction(ControllerConstants.SearchPensionRegulatorActionName,
                ControllerConstants.SearchPensionRegulatorControllerName);
        }

        return RedirectToAction(ControllerConstants.SearchForOrganisationActionName,
            ControllerConstants.SearchOrganisationControllerName);
    }

    private async Task<OrchestratorResponse<RenameEmployerAccountViewModel>> GetRenameViewModel(string hashedAccountId)
    {
        var vm = await _employerAccountOrchestrator.GetRenameEmployerAccountViewModel(hashedAccountId, GetUserId());
        return vm;
    }

    private string GetUserId()
    {
        var userIdClaim = HttpContext.User.FindFirstValue(ControllerConstants.UserRefClaimKeyName);
        return userIdClaim ?? "";
    }

    private void PopulateViewBagWithExternalUserId()
    {
        var externalUserId = HttpContext.User.FindFirstValue(ControllerConstants.UserRefClaimKeyName);
        if (externalUserId != null)
            ViewBag.UserId = externalUserId;
    }

    private async Task<IActionResult> CreateAccountFromCookieData()
    {
        var enteredData = _employerAccountOrchestrator.GetCookieData();

        if (enteredData == null)
        {
            // N.B CHANGED THIS FROM SelectEmployer which went nowhere.
            _employerAccountOrchestrator.DeleteCookieData();

            return RedirectToAction(ControllerConstants.SearchForOrganisationActionName,
                ControllerConstants.SearchOrganisationControllerName);
        }

        var request = new CreateAccountModel
        {
            UserId = GetUserId(),
            OrganisationType = enteredData.EmployerAccountOrganisationData.OrganisationType,
            OrganisationReferenceNumber = enteredData.EmployerAccountOrganisationData.OrganisationReferenceNumber,
            OrganisationName = enteredData.EmployerAccountOrganisationData.OrganisationName,
            OrganisationAddress = enteredData.EmployerAccountOrganisationData.OrganisationRegisteredAddress,
            OrganisationDateOfInception = enteredData.EmployerAccountOrganisationData.OrganisationDateOfInception,
            PayeReference = enteredData.EmployerAccountPayeRefData.PayeReference,
            AccessToken = enteredData.EmployerAccountPayeRefData.AccessToken,
            RefreshToken = enteredData.EmployerAccountPayeRefData.RefreshToken,
            OrganisationStatus =
                string.IsNullOrWhiteSpace(enteredData.EmployerAccountOrganisationData.OrganisationStatus)
                    ? null
                    : enteredData.EmployerAccountOrganisationData.OrganisationStatus,
            EmployerRefName = enteredData.EmployerAccountPayeRefData.EmployerRefName,
            PublicSectorDataSource = enteredData.EmployerAccountOrganisationData.PublicSectorDataSource,
            Sector = enteredData.EmployerAccountOrganisationData.Sector,
            HashedAccountId = _accountCookieStorage.Get(_hashedAccountIdCookieName),
            Aorn = enteredData.EmployerAccountPayeRefData.AORN
        };

        var response = await _employerAccountOrchestrator.CreateOrUpdateAccount(request, HttpContext);

        if (response.Status == HttpStatusCode.BadRequest)
        {
            response.Status = HttpStatusCode.OK;
            response.FlashMessage = new FlashMessageViewModel
                { Headline = "There was a problem creating your account" };
            return RedirectToAction(ControllerConstants.SummaryActionName);
        }

        _employerAccountOrchestrator.DeleteCookieData();

        var returnUrlCookie = _returnUrlCookieStorageService.Get(ReturnUrlCookieName);
        _accountCookieStorage.Delete(_hashedAccountIdCookieName);

        _returnUrlCookieStorageService.Delete(ReturnUrlCookieName);

        if (returnUrlCookie != null && !string.IsNullOrWhiteSpace(returnUrlCookie.Value))
            return Redirect(returnUrlCookie.Value);

        return RedirectToRoute(RouteNames.OrganisationAndPayeAddedSuccess,
            new { hashedAccountId = response.Data.EmployerAgreement.HashedAccountId });
    }

    private IActionResult HandleChangeAccountName(string hashedAccountId, RenameEmployerAccountViewModel vm, OrchestratorResponse<RenameEmployerAccountViewModel> response)
    {
        var validator = new RenameEmployerAccountViewModelValidator(RenameAccountType.RegistrationChangeAccountName);
        var validationResult = validator.Validate(vm);

        if (!validationResult.IsValid)
        {
            foreach (var validationError in validationResult.Errors)
            {
                vm.ErrorDictionary.Add(validationError.PropertyName, validationError.ErrorMessage);
            }
        }

        if (vm.ErrorDictionary.Count > 0)
        {
            response.Data = vm;
            response.Status = HttpStatusCode.BadRequest;

            return View(response);
        }

        return RedirectToRoute(RouteNames.AccountNameConfirm,
            new { hashedAccountId, NewAccountName = Uri.EscapeDataString(vm.NewName) });
    }

    private async Task<IActionResult> HandleSetEmployerAccountNameAsync(string hashedAccountId, RenameEmployerAccountViewModel vm)
    {
        var userIdClaim = HttpContext.User.FindFirstValue(ControllerConstants.UserRefClaimKeyName);
        var response = await _employerAccountOrchestrator.SetEmployerAccountName(hashedAccountId, vm, userIdClaim);

        if (response.Status == HttpStatusCode.OK)
        {
            return RedirectToRoute(
                vm.NameConfirmed ? RouteNames.AccountNameSuccess : RouteNames.AccountNameConfirmSuccess,
                new { hashedAccountId });
        }

        response.Data = vm;
        return View(response);
    }
}