using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerAccounts.Api.Authorization;
using SFA.DAS.EmployerAccounts.Api.Types;
using SFA.DAS.EmployerAccounts.Queries.GetAccountHistoryByPayeRef;

namespace SFA.DAS.EmployerAccounts.Api.Controllers;

[Route("api/accounthistories")]
public class AccountHistoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountHistoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Route("", Name = "GetAccountHistoryByRef")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    [HttpGet]
    public async Task<IActionResult> GetAccountHistoryByRef([FromQuery] string payeRef, CancellationToken cancellationToken)
    {
        var decodedPayeRef = Uri.UnescapeDataString(payeRef);

        var payeSchemeResult = await _mediator.Send(new GetAccountHistoryByPayeRefQuery { Ref = decodedPayeRef }, cancellationToken);

        if (payeSchemeResult == null)
        {
            return NotFound();
        }

        var payeAccount = new AccountHistory
        {
            AccountId = payeSchemeResult.AccountId,
            AddedDate = payeSchemeResult.AddedDate,
            RemovedDate = payeSchemeResult.RemovedDate
        };

        return Ok(payeAccount);
    }
}