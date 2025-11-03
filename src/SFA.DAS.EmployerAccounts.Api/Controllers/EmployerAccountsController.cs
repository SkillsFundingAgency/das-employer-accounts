using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Api.Authorization;
using SFA.DAS.EmployerAccounts.Api.Orchestrators;
using SFA.DAS.EmployerAccounts.Api.Types;
using SFA.DAS.EmployerAccounts.Commands.AcknowledgeTrainingProviderTask;
using SFA.DAS.EmployerAccounts.Commands.CreateAccount;
using SFA.DAS.EmployerAccounts.Commands.SignEmployerAgreementWithOutAudit;
using SFA.DAS.EmployerAccounts.Commands.UpsertRegisteredUser;
using SFA.DAS.EmployerAccounts.Exceptions;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Api.Controllers;

[Route("api/accounts")]
[ApiController]
public class EmployerAccountsController(AccountsOrchestrator orchestrator, IEncodingService encodingService, IMediator mediator, ILogger<EmployerAccountsController> logger)
    : ControllerBase
{
    [Route("", Name = "AccountsIndex")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    [HttpGet]
    public async Task<IActionResult> GetAccounts(string toDate = null, int pageSize = 1000, int pageNumber = 1)
    {
        var result = await orchestrator.GetAccounts(toDate, pageSize, pageNumber);

        foreach (var account in result.Data)
        {
            account.Href = Url.RouteUrl("GetAccountById", new { accountId = account.AccountId });
        }

        return Ok(result);
    }

    [Route("{accountId:long}", Name = "GetAccountById")]
    [Authorize(Policy = ApiRoles.ReadAllAccountUsers)]
    [HttpGet]
    public async Task<IActionResult> GetAccount(long accountId)
    {
        var isDecoded = encodingService.TryDecode(accountId.ToString(), EncodingType.AccountId, out _);

        if (isDecoded)
        {
            return await GetAccount(accountId.ToString());
        }

        var result = await orchestrator.GetAccount(accountId);
        if (result == null) return NotFound();

        result.LegalEntities.ForEach(x => CreateGetLegalEntityLink(accountId, x));
        result.PayeSchemes.ForEach(x => CreateGetPayeSchemeLink(accountId, x));
        return Ok(result);
    }

    [Route("{hashedAccountId}", Name = "GetAccount")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    [HttpGet]
    public async Task<IActionResult> GetAccount(string hashedAccountId)
    {
        var accountId = encodingService.Decode(hashedAccountId, EncodingType.AccountId);
        var result = await orchestrator.GetAccount(accountId);

        if (result == null) return NotFound();

        result.LegalEntities.ForEach(x => CreateGetLegalEntityLink(accountId, x));
        result.PayeSchemes.ForEach(x => CreateGetPayeSchemeLink(accountId, x));
        return Ok(result);
    }

    [Route("{hashedAccountId}/users", Name = "GetAccountUsers")]
    [Authorize(Policy = ApiRoles.ReadAllAccountUsers)]
    [HttpGet]
    public async Task<IActionResult> GetAccountUsers(string hashedAccountId)
    {
        var accountId = encodingService.Decode(hashedAccountId, EncodingType.AccountId);
        var result = await orchestrator.GetAccountTeamMembers(accountId);
        return Ok(result);
    }

    [Route("internal/{accountId}/users", Name = "GetAccountUsersByInternalAccountId")]
    [Authorize(Policy = ApiRoles.ReadAllAccountUsers)]
    [HttpGet]
    public async Task<IActionResult> GetAccountUsers(long accountId)
    {
        var result = await orchestrator.GetAccountTeamMembers(accountId);
        return Ok(result);
    }

    [Route("internal/{accountId}/users/which-receive-notifications", Name = "GetAccountUsersByInteralIdWhichReceiveNotifications")]
    [Authorize(Policy = ApiRoles.ReadAllAccountUsers)]
    [HttpGet]
    public async Task<IActionResult> GetAccountUsersWhichReceiveNotifications(long accountId)
    {
        var result = await orchestrator.GetAccountTeamMembersWhichReceiveNotifications(accountId);
        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(Models.Account.CreateEmployerAccountViaProviderResponseModel), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateEmployerAccountViaProviderRequest([FromBody] Models.Account.CreateEmployerAccountViaProviderRequestModel model, CancellationToken cancellationToken)
    {
        try
        {
            UpsertRegisteredUserCommand upsertRegisteredUserCommand = new()
            {
                CorrelationId = model.RequestId,
                EmailAddress = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserRef = model.UserRef.ToString()
            };

            await mediator.Send(upsertRegisteredUserCommand, cancellationToken);

            CreateAccountCommand createAccountCommand = new()
            {
                IsViaProviderRequest = true,
                CorrelationId = model.RequestId,
                ExternalUserId = model.UserRef.ToString(),
                OrganisationType = OrganisationType.PensionsRegulator,
                OrganisationName = model.EmployerOrganisationName,
                OrganisationAddress = model.EmployerAddress,
                PayeReference = model.EmployerPaye,
                Aorn = model.EmployerAorn,
                OrganisationReferenceNumber = model.EmployerOrganisationReferenceNumber,
                OrganisationStatus = "active",
                EmployerRefName = model.EmployerOrganisationName
            };
            CreateAccountCommandResponse createAccountCommandResponse = await mediator.Send(createAccountCommand, cancellationToken);

            SignEmployerAgreementWithoutAuditCommand signEmployerAgreementWithoutAuditCommand = new(createAccountCommandResponse.AgreementId, createAccountCommandResponse.User, model.RequestId);
            await mediator.Send(signEmployerAgreementWithoutAuditCommand, cancellationToken);

            AcknowledgeTrainingProviderTaskCommand acknowledgeTrainingProviderTaskCommand = new(createAccountCommandResponse.AccountId);
            await mediator.Send(acknowledgeTrainingProviderTaskCommand, cancellationToken);

            return CreatedAtAction(
                nameof(GetAccount),
                new { createAccountCommandResponse.AccountId },
                new Models.Account.CreateEmployerAccountViaProviderResponseModel(createAccountCommandResponse.AccountId, createAccountCommandResponse.AccountLegalEntityId));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Exception occurred whilst processing {ActionName} action.", nameof(CreateEmployerAccountViaProviderRequest));
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }

    [Route("acknowledge-training-provider-task", Name = "AcknowledgeTrainingProviderTask")]
    [Authorize(Policy = ApiRoles.ReadAllAccountUsers)]
    [HttpPatch]
    public async Task<IActionResult> AcknowledgeTrainingProviderTask([FromBody] AcknowledgeTrainingProviderTaskCommand command)
    {
        try
        {
            await orchestrator.AcknowledgeTrainingProviderTask(command.AccountId);
            return Ok();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Exception occurred whilst processing {ActionName} action.", nameof(AcknowledgeTrainingProviderTask));
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }

    [Route("updated", Name = "GetAccountsUpdated")]
    [Authorize(Policy = ApiRoles.ReadAllAccountUsers)]
    [HttpGet]
    public async Task<IActionResult> GetAccountsUpdated([FromQuery] DateTime sinceDate, int pageNumber = 1, int pageSize = 1000)
    {
        try
        {
            var result = await orchestrator.GetAccountsUpdated(sinceDate, pageNumber, pageSize);
            return Ok(result);
        }
        catch (InvalidRequestException ex)
        {
            logger.LogError(ex, "InvalidRequestException occurred whilst processing {ActionName} action.", nameof(GetAccountsUpdated));
            return new StatusCodeResult((int)HttpStatusCode.BadRequest);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Exception occurred whilst processing {ActionName} action.", nameof(GetAccountsUpdated));
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }

    [Route("search")]
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> SearchAccounts([FromQuery] string employerName)
    {
        if (string.IsNullOrEmpty(employerName))
        {
            return BadRequest();
        }
        
        var result = await mediator.Send(new Queries.SearchEmployerAccountsByName.SearchEmployerAccountsByNameQuery { EmployerName = employerName });
        return Ok(result);
    }

    private void CreateGetLegalEntityLink(long accountId, Resource legalEntity)
    {
        legalEntity.Href = Url.RouteUrl("GetLegalEntity", new { accountId = accountId, legalEntityId = legalEntity.Id });
    }

    private void CreateGetPayeSchemeLink(long accountId, Resource payeScheme)
    {
        payeScheme.Href = Url.RouteUrl("GetPayeScheme", new { accountId = accountId, payeSchemeRef = WebUtility.UrlEncode(payeScheme.Id) });
    }
}
