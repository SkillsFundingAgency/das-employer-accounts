using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerAccounts.Api.Types;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAgreementById;
using SFA.DAS.EmployerAccounts.Queries.GetEmployerAgreementsByAccountId;
using SFA.DAS.EmployerAccounts.Queries.GetMinimumSignedAgreementVersion;

namespace SFA.DAS.EmployerAccounts.Api.Orchestrators;

public class AgreementOrchestrator(IMediator mediator, ILogger<AgreementOrchestrator> logger, IMapper mapper)
{
    public async Task<EmployerAgreementView> GetAgreement(long agreementId)
    {
        var response = await mediator.Send(new GetEmployerAgreementByIdRequest
        {
            AgreementId = agreementId
        });

        return mapper.Map<EmployerAgreementView>(response.EmployerAgreement);
    }

    public async Task<int> GetMinimumSignedAgreemmentVersion(long accountId)
    {
        logger.LogInformation("Requesting minimum signed agreement version for account {AccountId}", accountId);

        var response = await mediator.Send(new GetMinimumSignedAgreementVersionQuery { AccountId = accountId });
        return response.MinimumSignedAgreementVersion;
    }

    public async Task<IEnumerable<EmployerAgreementView>> GetAgreements(long accountId)
    {
        var response = await mediator.Send(new GetEmployerAgreementsByAccountIdRequest { AccountId = accountId });

        if (response.EmployerAgreements == null || !response.EmployerAgreements.Any())
        {
            return [];
        }

        return response.EmployerAgreements?.Select(x => new EmployerAgreementView
        {
            Id = x.Id,
            AccountId = accountId,
            Acknowledged = x.Acknowledged.GetValueOrDefault(),
            SignedDate = x.SignedDate,
        });
    }
}