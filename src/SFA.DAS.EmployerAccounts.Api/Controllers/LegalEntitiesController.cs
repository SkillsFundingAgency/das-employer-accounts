using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerAccounts.Api.Authorization;
using SFA.DAS.EmployerAccounts.Api.Extensions;
using SFA.DAS.EmployerAccounts.Api.Mappings;
using SFA.DAS.EmployerAccounts.Api.Responses;
using SFA.DAS.EmployerAccounts.Api.Types;
using SFA.DAS.EmployerAccounts.Exceptions;
using SFA.DAS.EmployerAccounts.Queries.GetAccountLegalEntitiesByHashedAccountId;
using SFA.DAS.EmployerAccounts.Queries.GetAllAccountLegalEntitiesByHashedAccountId;
using SFA.DAS.EmployerAccounts.Queries.GetLegalEntity;

namespace SFA.DAS.EmployerAccounts.Api.Controllers;

[Route("api/accounts/{accountId}/legalentities")]
[Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
public class LegalEntitiesController(IMediator mediator, ILogger<LegalEntitiesController> logger) : ControllerBase
{
    [Route("", Name = "GetLegalEntities")]
    [HttpGet]
    public async Task<IActionResult> GetLegalEntities(long accountId, bool includeDetails = false)
    {
        GetAccountLegalEntitiesByHashedAccountIdResponse result;

        try
        {
            result = await mediator.Send(
                new GetAccountLegalEntitiesByHashedAccountIdRequest
                {
                    AccountId = accountId
                });
        }
        catch (InvalidRequestException)
        {
            return NotFound();
        }

        if (result.LegalEntities.Count == 0)
        {
            return NotFound();
        }

        if (!includeDetails)
        {
            var resources = new List<Resource>();

            foreach (var legalEntity in result.LegalEntities)
            {
                resources
                    .Add(
                        new Resource
                        {
                            Id = legalEntity.LegalEntityId.ToString(),
                            Href = Url.RouteUrl("GetLegalEntity",
                                new { hashedAccountId = accountId, legalEntityId = legalEntity.LegalEntityId })
                        });
            }

            return Ok(new ResourceList(resources));
        }

        var model = result.LegalEntities
            .Select(entity => LegalEntityMapping.MapFromAccountLegalEntity(entity, null, false))
            .ToList();

        return Ok(model);
    }

    [Route("GetAll", Name = "GetAllLegalEntities")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(GetAllAccountLegalEntitiesResponse), StatusCodes.Status200OK)]
    public async Task<IResult> GetAllLegalEntities(
        [FromRoute, Required] long accountId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortColumn = nameof(AccountLegalEntity.Name),
        [FromQuery] bool isAscending = false,
        CancellationToken token = default)
    {
        try
        {
            var response = await mediator.Send(new GetAllAccountLegalEntitiesByHashedAccountIdQuery(accountId, pageNumber, pageSize, sortColumn, isAscending), token);

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

    [HttpGet]
    [Route("{legalEntityId}", Name = "GetLegalEntity")]
    public async Task<IActionResult> GetLegalEntity(long accountId, long legalEntityId, bool includeAllAgreements = false)
    {
        var response = await mediator.Send(request: new GetLegalEntityQuery(accountId, legalEntityId));

        var model = LegalEntityMapping.MapFromAccountLegalEntity(response.LegalEntity, response.LatestAgreement,
            includeAllAgreements);

        if(model == null)
        {
            return NotFound();
        }

        return Ok(model);
    }
}