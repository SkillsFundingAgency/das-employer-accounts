using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerAccounts.Api.Authorization;
using SFA.DAS.EmployerAccounts.Api.Orchestrators;
using SFA.DAS.EmployerAccounts.Api.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Api.Controllers;

[Route("api/accounts")]
[ApiController]
public class EmployerAccountsController : ControllerBase
{
    private readonly AccountsOrchestrator _orchestrator;
    private readonly IEncodingService _encodingService;

    public EmployerAccountsController(AccountsOrchestrator orchestrator, IEncodingService encodingService)
    {
        _orchestrator = orchestrator;
        _encodingService = encodingService;
    }

    [Route("", Name = "AccountsIndex")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    [HttpGet]
    public async Task<IActionResult> GetAccounts(string toDate = null, int pageSize = 1000, int pageNumber = 1)
    {
        var result = await _orchestrator.GetAccounts(toDate, pageSize, pageNumber);

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
        var isDecoded = _encodingService.TryDecode(accountId.ToString(), EncodingType.AccountId, out _);

        if (isDecoded)
        {
            return await GetAccount(accountId.ToString());
        }
        
        var result = await _orchestrator.GetAccount(accountId);
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
        var accountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId);
        var result = await _orchestrator.GetAccount(accountId);

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
        var accountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId);
        var result = await _orchestrator.GetAccountTeamMembers(accountId);
        return Ok(result);
    }

    [Route("internal/{accountId}/users", Name = "GetAccountUsersByInternalAccountId")]
    [Authorize(Policy = ApiRoles.ReadAllAccountUsers)]
    [HttpGet]
    public async Task<IActionResult> GetAccountUsers(long accountId)
    {
        var result = await _orchestrator.GetAccountTeamMembers(accountId);
        return Ok(result);
    }

    [Route("internal/{accountId}/users/which-receive-notifications", Name = "GetAccountUsersByInteralIdWhichReceiveNotifications")]
    [Authorize(Policy = ApiRoles.ReadAllAccountUsers)]
    [HttpGet]
    public async Task<IActionResult> GetAccountUsersWhichReceiveNotifications(long accountId)
    {
        var result = await _orchestrator.GetAccountTeamMembersWhichReceiveNotifications(accountId);
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