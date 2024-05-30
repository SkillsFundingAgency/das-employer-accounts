using System;
using System.Net;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerAccounts.Api.Authorization;
using SFA.DAS.EmployerAccounts.Commands.SupportChangeTeamMemberRole;
using SFA.DAS.EmployerAccounts.Commands.SupportCreateInvitation;
using SFA.DAS.EmployerAccounts.Commands.SupportResendInvitationCommand;

namespace SFA.DAS.EmployerAccounts.Api.Controllers;

[Route("api/support")]
[Authorize(Policy = ApiRoles.ReadUserAccounts)]
public class SupportController(IMediator mediator, ILogger<SupportController> logger) : ControllerBase
{
    [HttpPost]
    [Route("change-role", Name = RouteNames.Support.ChangeRole)]
    public async Task<IActionResult> ChangeRole([FromBody] SupportChangeTeamMemberRoleCommand command)
    {
        try
        {
            await mediator.Send(command);
            return Ok();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error in {Controller}.{Action}", nameof(SupportController), nameof(ChangeRole));
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }
    
    [HttpPost]
    [Route("send-invitation", Name = RouteNames.Support.SendInvitation)]
    public async Task<IActionResult> SendInvitation([FromBody] SupportCreateInvitationCommand command)
    {
        try
        {
            await mediator.Send(command);
            return Ok();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error in {Controller}.{Action}", nameof(SupportController), nameof(SendInvitation));
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }
 
    [HttpPost]
    [Route("resend-invitation", Name = RouteNames.Support.ResendInvitation)]
    public async Task<IActionResult> ResendInvitation([FromBody] SupportResendInvitationCommand command)
    {
        command.Email = WebUtility.UrlDecode(command.Email);
        
        try
        {
            await mediator.Send(command);
            return Ok();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error in {Controller}.{Action}", nameof(SupportController), nameof(ResendInvitation));
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }
}