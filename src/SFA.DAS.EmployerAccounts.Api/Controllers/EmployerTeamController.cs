using System;
using System.Net;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerAccounts.Api.Authorization;
using SFA.DAS.EmployerAccounts.Commands.ChangeTeamMemberRole;
using SFA.DAS.EmployerAccounts.Commands.ResendInvitation;

namespace SFA.DAS.EmployerAccounts.Api.Controllers;

[Route("api/team")]
[Authorize(Policy = ApiRoles.ReadUserAccounts)]
public class EmployerTeamController(IMediator mediator, ILogger<EmployerTeamController> logger) : ControllerBase
{
    [HttpPut]
    [Route("change-role", Name = RouteNames.EmployerTeam.ChangeRole)]
    public async Task<IActionResult> ChangeRole([FromBody] ChangeTeamMemberRoleCommand command)
    {
        try
        {
            await mediator.Send(command);
            return Ok();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error in {Controller}.{Action}", nameof(EmployerUserController), nameof(ChangeRole));
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }

    [HttpPost]
    [Route("resend-invitation", Name = RouteNames.EmployerTeam.ResendInvitation)]
    public async Task<IActionResult> ResendInvitation([FromBody] ResendInvitationCommand command)
    {
        command.Email = WebUtility.HtmlDecode(command.Email);
        
        try
        {
            await mediator.Send(command);
            return Ok();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error in {Controller}.{Action}", nameof(EmployerUserController), nameof(ResendInvitation));
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }
}