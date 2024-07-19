using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerAccounts.Api.Authorization;
using SFA.DAS.EmployerAccounts.Api.Orchestrators;
using SFA.DAS.EmployerAccounts.Api.Types;
using SFA.DAS.EmployerAccounts.Commands.AcknowledgeTrainingProviderTask;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Api.Controllers;

[Route("api/accounts")]
[ApiController]
public class EmployerAccountsController(AccountsOrchestrator orchestrator, IEncodingService encodingService, ILogger<EmployerAccountsController> logger)
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
            account.Href = Url.RouteUrl("GetAccount", new { hashedAccountId = account.AccountHashId });
        }

        return Ok(result);
    }

    [Route("{hashedAccountId}", Name = "GetAccount")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    [HttpGet]
    public async Task<IActionResult> GetAccount(string hashedAccountId)
    {
        var result = await orchestrator.GetAccount(hashedAccountId);

        if (result == null) return NotFound();

        result.LegalEntities.ForEach(x => CreateGetLegalEntityLink(hashedAccountId, x));
        result.PayeSchemes.ForEach(x => CreateGetPayeSchemeLink(hashedAccountId, x));
        return Ok(result);
    }

    [Route("{accountId:long}", Name = "GetAccountById")]
    [Authorize(Policy = ApiRoles.ReadAllAccountUsers)]
    [HttpGet]
    public async Task<IActionResult> GetAccountById(long accountId)
    {
        var isDecoded = encodingService.TryDecode(accountId.ToString(), EncodingType.AccountId, out _);

        if (isDecoded)
        {
            return await GetAccount(accountId.ToString());
        }

        var result = await orchestrator.GetAccountById(accountId);
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

    private void CreateGetLegalEntityLink(string hashedAccountId, Resource legalEntity)
    {
        legalEntity.Href = Url.RouteUrl("GetLegalEntity", new { hashedAccountId, legalEntityId = legalEntity.Id });
    }

    private void CreateGetPayeSchemeLink(string hashedAccountId, Resource payeScheme)
    {
        payeScheme.Href = Url.RouteUrl("GetPayeScheme", new { hashedAccountId, payeSchemeRef = WebUtility.UrlEncode(payeScheme.Id) });
    }
}