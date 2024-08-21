using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerAccounts.Api.Authorization;
using SFA.DAS.EmployerAccounts.Api.Types;
using SFA.DAS.EmployerAccounts.Queries.GetPayeSchemeAccountByRef;

namespace SFA.DAS.EmployerAccounts.Api.Controllers;

[Route("paye")]
public class PayeController : ControllerBase
{
    private readonly IMediator _mediator;

    public PayeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Route("{payeSchemeRef}/accounthistory", Name = "GetPayeSchemeAccount")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    [HttpGet]
    public async Task<IActionResult> GetAccountHistoryByRef([FromRoute] string payeSchemeRef, CancellationToken cancellationToken)
    {
        var decodedPayeSchemeRef = Uri.UnescapeDataString(payeSchemeRef);

        var payeSchemeResult = await _mediator.Send(new GetPayeSchemeAccountByRefQuery { Ref = decodedPayeSchemeRef }, cancellationToken);

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