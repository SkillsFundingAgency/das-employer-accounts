using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerAccounts.Api.Authorization;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAgreementTemplates;

namespace SFA.DAS.EmployerAccounts.Api.Controllers;

[Route("api/EmployerAgreementTemplates")]
[Authorize(Policy = ApiRoles.ReadAllEmployerAgreements)]
public class EmployerAgreementTemplatesController : Controller
{
    private readonly IMediator _mediator;

    public EmployerAgreementTemplatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetEmployerAgreementTemplatesRequest(), cancellationToken);
        return Ok(result);
    }
}
