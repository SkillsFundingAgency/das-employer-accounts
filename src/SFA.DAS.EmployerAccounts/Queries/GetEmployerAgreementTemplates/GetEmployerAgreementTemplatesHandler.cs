using System.Threading;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;

namespace SFA.DAS.EmployerAccounts.Queries.GetEmployerAgreementTemplates;

public class GetEmployerAgreementTemplatesHandler(IEmployerAgreementTemplatesRepository _employerAgreementTemplatesRepository) : IRequestHandler<GetEmployerAgreementTemplatesRequest, GetEmployerAgreementTemplatesResponse>
{
    public async Task<GetEmployerAgreementTemplatesResponse> Handle(GetEmployerAgreementTemplatesRequest request, CancellationToken cancellationToken)
    {
        var templates = await _employerAgreementTemplatesRepository.GetEmployerAgreementTemplates().ConfigureAwait(false);

        var employerAgreementTemplates = templates.Select(template => new EmployerAgreementTemplate
        {
            Id = template.Id,
            PartialViewName = template.PartialViewName,
            CreatedDate = template.CreatedDate.GetValueOrDefault(),
            VersionNumber = template.VersionNumber,
            AgreementType = template.AgreementType,
            PublishedDate = template.PublishedDate
        })
            .ToList();

        return new GetEmployerAgreementTemplatesResponse
        {
            EmployerAgreementTemplates = employerAgreementTemplates
        };
    }
}