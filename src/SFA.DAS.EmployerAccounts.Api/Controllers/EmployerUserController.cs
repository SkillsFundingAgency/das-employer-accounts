using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerAccounts.Api.Authorization;
using SFA.DAS.EmployerAccounts.Api.Orchestrators;

namespace SFA.DAS.EmployerAccounts.Api.Controllers;

[Route("api/user/{userRef}")]
public class EmployerUserController(IUsersOrchestrator orchestrator)
    : ControllerBase
{
    [Route("accounts", Name = "Accounts")]
    [Authorize(Policy = ApiRoles.ReadUserAccounts)]
    [HttpGet]
    public async Task<IActionResult> GetUserAccounts(string userRef)
    {
        return Ok(await orchestrator.GetUserAccounts(userRef));
    }
}