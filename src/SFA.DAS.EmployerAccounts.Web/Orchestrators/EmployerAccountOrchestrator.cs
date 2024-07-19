using SFA.DAS.EmployerAccounts.Commands.AcknowledgeTrainingProviderTask;
using SFA.DAS.EmployerAccounts.Commands.AddPayeToAccount;
using SFA.DAS.EmployerAccounts.Commands.CreateAccount;
using SFA.DAS.EmployerAccounts.Commands.CreateAccountComplete;
using SFA.DAS.EmployerAccounts.Commands.CreateLegalEntity;
using SFA.DAS.EmployerAccounts.Commands.RenameEmployerAccount;
using SFA.DAS.EmployerAccounts.Queries.GetAccountPayeSchemes;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAccount;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAccountDetail;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAgreementsByAccountId;
using SFA.DAS.EmployerAccounts.Queries.GetUserAccounts;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Web.Orchestrators;

public class EmployerAccountOrchestrator : EmployerVerificationOrchestratorBase
{
    private readonly IEmployerAccountService _employerAccountService;
    private readonly ILogger<EmployerAccountOrchestrator> _logger;
    private readonly IEncodingService _encodingService;
    private readonly IUrlActionHelper _urlHelper;
    private const string CookieName = "sfa-das-employerapprenticeshipsservice-employeraccount";

    //Needed for tests
    protected EmployerAccountOrchestrator()
    {
    }

    public EmployerAccountOrchestrator(
        IEmployerAccountService employerAccountService,
        IMediator mediator,
        ILogger<EmployerAccountOrchestrator> logger,
        ICookieStorageService<EmployerAccountData> cookieService,
        EmployerAccountsConfiguration configuration,
        IEncodingService encodingService,
        IUrlActionHelper urlHelper)
        : base(mediator, cookieService, configuration)
    {
        _employerAccountService = employerAccountService;
        _logger = logger;
        _encodingService = encodingService;
        _urlHelper = urlHelper;
    }

    public async Task<OrchestratorResponse<EmployerAccountViewModel>> GetEmployerAccount(long accountId)
    {
        var response = await Mediator.Send(new GetEmployerAccountByIdQuery
        {
            AccountId = accountId
        });

        return new OrchestratorResponse<EmployerAccountViewModel>
        {
            Data = new EmployerAccountViewModel
            {
                Name = response.Account.Name,
                HashedId = response.Account.HashedId
            }
        };
    }

    public virtual async Task<OrchestratorResponse<RenameEmployerAccountViewModel>> GetRenameEmployerAccountViewModel(
        string hashedAccountId, string userId)
    {
        var accountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId);
        var response = await Mediator.Send(new GetEmployerAccountByIdQuery
        {
            AccountId = accountId,
            UserId = userId
        });

        return new OrchestratorResponse<RenameEmployerAccountViewModel>
        {
            Data = new RenameEmployerAccountViewModel
            {
                LegalEntityName = response.Account.AccountLegalEntities.OrderBy(x => x.Created).First()?.Name,
                CurrentName = response.Account.Name,
                NewName = string.Empty
            }
        };
    }

    public virtual async Task<OrchestratorResponse<RenameEmployerAccountViewModel>> SetEmployerAccountName(
        string hashedAccountId, RenameEmployerAccountViewModel model, string userId)
    {
        var response = new OrchestratorResponse<RenameEmployerAccountViewModel> { Data = model };
        _ = await RenameEmployerAccount(hashedAccountId, model, userId);

        return response;
    }

    public virtual async Task<OrchestratorResponse<RenameEmployerAccountViewModel>> RenameEmployerAccount(
        string hashedAccountId, RenameEmployerAccountViewModel model, string userId)
    {
        var accountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId);
        var response = new OrchestratorResponse<RenameEmployerAccountViewModel> { Data = model };

        var userRoleResponse = await GetUserAccountRole(accountId, userId);

        if (!userRoleResponse.UserRole.Equals(Role.Owner))
        {
            return new OrchestratorResponse<RenameEmployerAccountViewModel>
            {
                Status = HttpStatusCode.Unauthorized
            };
        }

        try
        {
            await Mediator.Send(new RenameEmployerAccountCommand
            {
                HashedAccountId = hashedAccountId,
                ExternalUserId = userId,
                NewName = (model.NewName ?? string.Empty).Trim()
            });

            model.CurrentName = model.NewName;
            model.NewName = string.Empty;
            response.Data = model;
            response.Status = HttpStatusCode.OK;
        }
        catch (InvalidRequestException ex)
        {
            response.Status = HttpStatusCode.BadRequest;
            response.Data.ErrorDictionary = ex.ErrorMessages;
            response.Exception = ex;
        }

        return response;
    }

    public virtual async Task<OrchestratorResponse<EmployerAgreementViewModel>> CreateOrUpdateAccount(
        CreateAccountModel model, HttpContext context)
    {
        if (string.IsNullOrWhiteSpace(model?.HashedAccountId?.Value))
        {
            return await CreateNewAccount(model);
        }

        return await UpdateExistingAccount(model);
    }

    private async Task<OrchestratorResponse<EmployerAgreementViewModel>> UpdateExistingAccount(CreateAccountModel model)
    {
        try
        {
            await AddPayeToExistingAccount(model);

            var agreementId = await AddLegalEntityToExistingAccount(model);
            var hashedAgreementId = _encodingService.Encode(agreementId, EncodingType.AccountId);

            await UpdateAccountNameToLegalEntityName(model);

            return new OrchestratorResponse<EmployerAgreementViewModel>
            {
                Data = new EmployerAgreementViewModel
                {
                    EmployerAgreement = new EmployerAgreementView
                    {
                        HashedAccountId = model.HashedAccountId.Value,
                        HashedAgreementId = hashedAgreementId,
                        Id = agreementId
                    }
                },
                Status = HttpStatusCode.OK
            };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Create Account Validation Error: {Message}", exception.Message);
            return new OrchestratorResponse<EmployerAgreementViewModel>
            {
                Data = new EmployerAgreementViewModel(),
                Status = HttpStatusCode.BadRequest,
                Exception = exception,
                FlashMessage = new FlashMessageViewModel()
            };
        }
    }

    private async Task UpdateAccountNameToLegalEntityName(CreateAccountModel model)
    {
        await Mediator.Send(new RenameEmployerAccountCommand
        {
            HashedAccountId = model.HashedAccountId.Value,
            ExternalUserId = model.UserId,
            NewName = model.OrganisationName
        });
    }

    private async Task<long> AddLegalEntityToExistingAccount(CreateAccountModel model)
    {
        var response = await Mediator.Send(new CreateLegalEntityCommand
        {
            HashedAccountId = model.HashedAccountId.Value,
            Code = model.OrganisationReferenceNumber,
            DateOfIncorporation = model.OrganisationDateOfInception,
            Status = model.OrganisationStatus,
            Source = model.OrganisationType,
            PublicSectorDataSource = Convert.ToByte(model.PublicSectorDataSource),
            Sector = model.Sector,
            Name = model.OrganisationName,
            Address = model.OrganisationAddress,
            ExternalUserId = model.UserId
        });

        return response.AgreementView.Id;
    }

    private async Task AddPayeToExistingAccount(CreateAccountModel model)
    {
        await Mediator.Send(new AddPayeToAccountCommand
        {
            HashedAccountId = model.HashedAccountId.Value,
            AccessToken = model.AccessToken,
            RefreshToken = model.RefreshToken,
            Empref = model.PayeReference,
            ExternalUserId = model.UserId,
            EmprefName = model.EmployerRefName,
            Aorn = model.Aorn
        });
    }

    private async Task<OrchestratorResponse<EmployerAgreementViewModel>> CreateNewAccount(CreateAccountModel model)
    {
        try
        {
            var result = await Mediator.Send(new CreateAccountCommand
            {
                ExternalUserId = model.UserId,
                OrganisationType = model.OrganisationType,
                OrganisationName = model.OrganisationName,
                OrganisationReferenceNumber = model.OrganisationReferenceNumber,
                OrganisationAddress = model.OrganisationAddress,
                OrganisationDateOfInception = model.OrganisationDateOfInception,
                PayeReference = model.PayeReference,
                AccessToken = model.AccessToken,
                RefreshToken = model.RefreshToken,
                OrganisationStatus = model.OrganisationStatus,
                EmployerRefName = model.EmployerRefName,
                PublicSectorDataSource = model.PublicSectorDataSource,
                Sector = model.Sector,
                Aorn = model.Aorn
            });

            return new OrchestratorResponse<EmployerAgreementViewModel>
            {
                Data = new EmployerAgreementViewModel
                {
                    EmployerAgreement = new EmployerAgreementView
                    {
                        HashedAccountId = result.HashedAccountId,
                        HashedAgreementId = result.HashedAgreementId
                    }
                },
                Status = HttpStatusCode.OK
            };
        }
        catch (InvalidRequestException ex)
        {
            _logger.LogError(ex, "Create Account Validation Error: {Message}", ex.Message);

            return new OrchestratorResponse<EmployerAgreementViewModel>
            {
                Data = new EmployerAgreementViewModel(),
                Status = HttpStatusCode.BadRequest,
                Exception = ex,
                FlashMessage = new FlashMessageViewModel()
            };
        }
    }

    public virtual OrchestratorResponse<SummaryViewModel> GetSummaryViewModel(HttpContext context)
    {
        var enteredData = GetCookieData();

        var model = new SummaryViewModel
        {
            OrganisationType = enteredData.EmployerAccountOrganisationData.OrganisationType,
            OrganisationName = enteredData.EmployerAccountOrganisationData.OrganisationName,
            RegisteredAddress = enteredData.EmployerAccountOrganisationData.OrganisationRegisteredAddress?.Split(','),
            OrganisationReferenceNumber = enteredData.EmployerAccountOrganisationData.OrganisationReferenceNumber,
            OrganisationDateOfInception = enteredData.EmployerAccountOrganisationData.OrganisationDateOfInception,
            PayeReference = enteredData.EmployerAccountPayeRefData.PayeReference,
            EmployerRefName = enteredData.EmployerAccountPayeRefData.EmployerRefName,
            EmpRefNotFound = enteredData.EmployerAccountPayeRefData.EmpRefNotFound,
            OrganisationStatus = enteredData.EmployerAccountOrganisationData.OrganisationStatus,
            PublicSectorDataSource = enteredData.EmployerAccountOrganisationData.PublicSectorDataSource,
            Sector = enteredData.EmployerAccountOrganisationData.Sector,
            NewSearch = enteredData.EmployerAccountOrganisationData.NewSearch,
            AORN = enteredData.EmployerAccountPayeRefData.AORN
        };

        return new OrchestratorResponse<SummaryViewModel>
        {
            Data = model
        };
    }

    public virtual EmployerAccountData GetCookieData()
    {
        return CookieService.Get(CookieName);
    }

    public virtual void DeleteCookieData()
    {
        CookieService.Delete(CookieName);
    }

    public virtual async Task<OrchestratorResponse<AccountTaskListViewModel>> GetCreateAccountTaskList(string hashedAccountId, string userRef)
    {
        var response = new OrchestratorResponse<AccountTaskListViewModel>();

        var userResponse = await Mediator.Send(new GetUserByRefQuery { UserRef = userRef });

        if (string.IsNullOrEmpty(hashedAccountId))
        {
            var existingTaskListViewModel = await GetFirstUserAccount(userRef);

            response.Data = existingTaskListViewModel;
        }
        else
        {
            var accountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId);
            var employerAccountTaskListResponse = await _employerAccountService.GetEmployerAccountTaskList(accountId, hashedAccountId);

            var accountResponse = await Mediator.Send(new GetEmployerAccountDetailByHashedIdQuery
            {
                HashedAccountId = hashedAccountId
            });

            var accountAgreementsResponse = await Mediator.Send(new GetEmployerAgreementsByAccountIdRequest
            {
                AccountId = accountId
            });

            if (accountResponse?.Account == null
                || accountAgreementsResponse.EmployerAgreements == null
                || !accountAgreementsResponse.EmployerAgreements.Any())
            {
                return new OrchestratorResponse<AccountTaskListViewModel> { Status = HttpStatusCode.NotFound };
            }

            var agreement = accountAgreementsResponse.EmployerAgreements.FirstOrDefault();
            var hasAgreement = agreement != null;

            response.Data = new AccountTaskListViewModel
            {
                HashedAccountId = hashedAccountId,
                HasPayeScheme = accountResponse?.Account?.PayeSchemes?.Any() ?? false,
                NameConfirmed = accountResponse?.Account?.NameConfirmed ?? false,
                PendingHashedAgreementId = _encodingService.Encode(agreement.Id, EncodingType.AccountId),
                AddTrainingProviderAcknowledged = accountResponse.Account.AddTrainingProviderAcknowledged ?? true
            };

            if (hasAgreement)
            {
                response.Data.AgreementAcknowledged = agreement.Acknowledged ?? true;
                response.Data.HasSignedAgreement = agreement.SignedDate.HasValue;
                response.Data.HasProviders = employerAccountTaskListResponse?.HasProviders ?? false;
                response.Data.HasProviderPermissions = employerAccountTaskListResponse?.HasProviderPermissions ?? false;

                if (!response.Data.AddTrainingProviderAcknowledged && response.Data.HasProviders && response.Data.HasProviderPermissions)
                {
                    await Mediator.Send(new AcknowledgeTrainingProviderTaskCommand(accountId));
                }
            }
        }

        response.Data.EditUserDetailsUrl = _urlHelper.EmployerProfileEditUserDetails() + $"?firstName={userResponse.User.FirstName}&lastName={userResponse.User.LastName}";
        response.Data.ProviderPermissionsUrl = _urlHelper.ProviderRelationshipsAction("providers") + $"?AccountTasks=true";

        return response;
    }

    private async Task<AccountTaskListViewModel> GetFirstUserAccount(string userRef)
    {
        var getAccountPayeResponse = new GetAccountPayeSchemesResponse();
        var getUserAccountsQueryResponse = await Mediator.Send(new GetUserAccountsQuery
        {
            UserRef = userRef
        });

        var firstAccount = getUserAccountsQueryResponse.Accounts == null || !getUserAccountsQueryResponse.Accounts.AccountList.Any()
            ? null
            : getUserAccountsQueryResponse.Accounts.AccountList.MinBy(x => x.CreatedDate);

        if (firstAccount != null)
        {
            getAccountPayeResponse = await Mediator.Send(new GetAccountPayeSchemesQuery
            {
                AccountId = firstAccount.Id
            });
        }

        return new AccountTaskListViewModel
        {
            HashedAccountId = firstAccount?.HashedId,
            HasPayeScheme = getAccountPayeResponse.PayeSchemes?.Any() ?? false,
            NameConfirmed = firstAccount?.NameConfirmed ?? false
        };
    }

    public async Task AcknowledgeTrainingProviderTask(string hashedAccountId, string externalUserId)
    {
        var accountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId);

        var accountResponse = await Mediator.Send(new GetEmployerAccountByIdQuery { AccountId = accountId, UserId = externalUserId });

        await Mediator.Send(new AcknowledgeTrainingProviderTaskCommand(accountId));

        var publicHashedAccountId = _encodingService.Encode(accountId, EncodingType.PublicAccountId);
        await Mediator.Send(new SendAccountTaskListCompleteNotificationCommand
        {
            AccountId = accountId,
            PublicHashedAccountId = publicHashedAccountId,
            HashedAccountId = hashedAccountId,
            ExternalUserId = externalUserId,
            OrganisationName = accountResponse.Account.Name
        });
    }
}