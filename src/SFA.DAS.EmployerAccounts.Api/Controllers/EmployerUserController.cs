using System;
using System.Net;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerAccounts.Api.Authorization;
using SFA.DAS.EmployerAccounts.Api.Orchestrators;
using SFA.DAS.EmployerAccounts.Commands.ChangeTeamMemberRole;

namespace SFA.DAS.EmployerAccounts.Api.Controllers;

[Route("api/user/{userRef}")]
public class EmployerUserController(IUsersOrchestrator orchestrator, IMediator mediator, ILogger<EmployerUserController> logger)
    : ControllerBase
{
    [Route("accounts", Name = "Accounts")]
    [Authorize(Policy = ApiRoles.ReadUserAccounts)]
    [HttpGet]
    public async Task<IActionResult> GetUserAccounts(string userRef)
    {
        return Ok(await orchestrator.GetUserAccounts(userRef));
    }
    
    [HttpPut]
    [Route("change-role", Name = "ChangeRole")]
    [Authorize(Policy = ApiRoles.ReadUserAccounts)]
    public async Task<IActionResult> ChangeRole([FromBody] ChangeTeamMemberRoleCommand command)
    {
        try
        {
            await mediator.Send(command);
            return Ok();
        }
        catch(Exception e)
        {
            logger.LogError(e,"Error in {Controller} PUT", nameof(EmployerUserController));
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }
}