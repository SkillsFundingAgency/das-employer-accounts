using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerAccounts.Api.Authorization;
using SFA.DAS.EmployerAccounts.Api.Extensions;
using SFA.DAS.EmployerAccounts.Api.Mappings;
using SFA.DAS.EmployerAccounts.Api.Requests;
using SFA.DAS.EmployerAccounts.Api.Responses;
using SFA.DAS.EmployerAccounts.Queries.GetAccountLegalEntities.Api;
using SFA.DAS.EmployerAccounts.Queries.GetAllAccountLegalEntitiesByHashedAccountId;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerAccounts.Api.Controllers;

[Authorize(Policy = ApiRoles.ReadUserAccounts)]
[Route("api/accountlegalentities")]
public class AccountLegalEntitiesController(
    IMediator mediator,
    ILogger<AccountLegalEntitiesController> logger) : ControllerBase
{

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GetAccountLegalEntitiesQuery query)
    {
        var response = await mediator.Send(query);
        return Ok(response.AccountLegalEntities);
    }

    [Route("GetAll", Name = "GetAllLegalEntities")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(GetAllAccountLegalEntitiesResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetAllLegalEntities(
        [FromBody] GetAllLegalEntitiesRequest request,
        CancellationToken token = default)
    {
        try
        {
            var response = await mediator.Send(new GetAllAccountLegalEntitiesByHashedAccountIdQuery(request.SearchTerm,
                    request.AccountIds,
                    request.PageNumber,
                    request.PageSize,
                    request.SortColumn,
                    request.IsAscending),
                token);

            var model = response.LegalEntities
                .Items
                .Select(entity => LegalEntityMapping.MapFromAccountLegalEntity(entity, null, false));

            return TypedResults.Ok(new GetAllAccountLegalEntitiesResponse(response.LegalEntities.ToPageInfo(), model));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to Get all legal entities by account Id : An error occurred");
            return Results.Problem(statusCode: (int)HttpStatusCode.InternalServerError);
        }
    }
}