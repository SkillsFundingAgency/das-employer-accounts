using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerAccounts.Api.Authorization;
using SFA.DAS.EmployerAccounts.Api.Types;
using SFA.DAS.EmployerAccounts.Queries.GetPayeAccountByRef;

namespace SFA.DAS.EmployerAccounts.Api.Controllers;

[Route("api/accounts/payeschemes")]
public class AccountPayeController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountPayeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Route("{payeSchemeRef}", Name = "GetPayeAccount")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    [HttpGet]
    public async Task<IActionResult> GetPayeAccountDetails([FromRoute] string payeSchemeRef, CancellationToken cancellationToken)
    {
        var decodedPayeSchemeRef = Uri.UnescapeDataString(payeSchemeRef);

        var payeAccountResult = await _mediator.Send(new GetPayeAccountByRefQuery { Ref = decodedPayeSchemeRef }, cancellationToken);

        if (payeAccountResult == null)
        {
            return NotFound();
        }

        var payeAccount = new PayeAccount
        {
            AccountId = payeAccountResult.AccountId,
            AddedDate = payeAccountResult.AddedDate,
            RemovedDate = payeAccountResult.RemovedDate
        };

        return Ok(payeAccount);
    }
}