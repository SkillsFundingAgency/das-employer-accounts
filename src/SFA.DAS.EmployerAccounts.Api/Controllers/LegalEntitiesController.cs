using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerAccounts.Api.Authorization;
using SFA.DAS.EmployerAccounts.Api.Mappings;
using SFA.DAS.EmployerAccounts.Api.Types;
using SFA.DAS.EmployerAccounts.Exceptions;
using SFA.DAS.EmployerAccounts.Queries.GetAccountLegalEntitiesByHashedAccountId;
using SFA.DAS.EmployerAccounts.Queries.GetLegalEntity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerAccounts.Api.Controllers;

[Route("api/accounts/{accountId}/legalentities")]
[Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
public class LegalEntitiesController(IMediator mediator) : ControllerBase
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