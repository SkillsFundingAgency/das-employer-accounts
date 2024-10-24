using System.Threading;
using SFA.DAS.EmployerAccounts.Data.Contracts;

namespace SFA.DAS.EmployerAccounts.Queries.GetEmployerAgreementTemplates;

public class GetEmployerAgreementTemplatesHandler : IRequestHandler<GetEmployerAgreementTemplatesRequest, GetEmployerAgreementTemplatesResponse>
{
    private readonly IEmployerAgreementTemplatesRepository _employerAgreementTemplatesRepository;


    public GetEmployerAgreementTemplatesHandler(
        IEmployerAgreementTemplatesRepository employerAgreementTemplatesRepository)
    {
        _employerAgreementTemplatesRepository = employerAgreementTemplatesRepository;

    }

    public async Task<GetEmployerAgreementTemplatesResponse> Handle(GetEmployerAgreementTemplatesRequest request, CancellationToken cancellationToken)
    {
        var templates = await _employerAgreementTemplatesRepository.GetEmployerAgreementTemplates().ConfigureAwait(false);

        return new GetEmployerAgreementTemplatesResponse
        {
            EmployerAgreementTemplates = templates.ToList()
        };
    }
}