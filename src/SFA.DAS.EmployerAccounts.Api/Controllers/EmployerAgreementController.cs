using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerAccounts.Api.Authorization;
using SFA.DAS.EmployerAccounts.Api.Orchestrators;
using SFA.DAS.EmployerAccounts.Api.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Api.Controllers;

[Route("api/accounts")]
public class EmployerAgreementController(
    AgreementOrchestrator orchestrator, 
    IEncodingService encodingService) : ControllerBase
{
    [Route("{hashedAccountId}/legalEntities/{hashedlegalEntityId}/agreements/{agreementId}", Name = "AgreementById")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAgreements)]
    [HttpGet]
    public async Task<IActionResult> GetAgreement(string agreementId)
    {
        var decodedAgreementId = encodingService.Decode(agreementId, EncodingType.AccountId);
        var response = await orchestrator.GetAgreement(decodedAgreementId);

        if (response == null)
        {
            return NotFound();
        }

        return Ok(response);
    }

    [Route("internal/{accountId}/minimum-signed-agreement-version", Name = "InternalGetMinimumSignedAgreemmentVersion")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAgreements)]
    [HttpGet]
    public async Task<IActionResult> GetMinimumSignedAgreemmentVersion(long accountId)
    {
        var result = await orchestrator.GetMinimumSignedAgreemmentVersion(accountId);
        return Ok(result);
    }
    
    [Route("{hashedAccountId}/signed-agreement-version", Name = "GetMinimumSignedAgreemmentVersion")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAgreements)]
    [HttpGet]
    public async Task<IActionResult> GetMinimumSignedAgreementVersionByHashedId(string hashedAccountId)
    {
        var accountId = encodingService.Decode(hashedAccountId, EncodingType.AccountId);
        var result = await orchestrator.GetMinimumSignedAgreemmentVersion(accountId);
        return Ok(new MinimumSignedAgreementResponse
        {
            MinimumSignedAgreementVersion = result
        });
    }

    [Route("{accountId}/agreements", Name = "GetAgreements")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAgreements)]
    [HttpGet]
    public async Task<IActionResult> GetAgreements(long accountId)
    {
        var result = await orchestrator.GetAgreements(accountId);
        return Ok(result);
    }
}