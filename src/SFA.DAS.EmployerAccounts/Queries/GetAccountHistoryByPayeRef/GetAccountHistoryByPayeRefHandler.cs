using System.Threading;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.PAYE;

namespace SFA.DAS.EmployerAccounts.Queries.GetAccountHistoryByPayeRef;
public class GetAccountHistoryByPayeRefHandler : IRequestHandler<GetAccountHistoryByPayeRefQuery, PayeScheme>
{
    private readonly IEmployerSchemesRepository _employerSchemesRepository;

    public GetAccountHistoryByPayeRefHandler(IEmployerSchemesRepository employerSchemesRepository)
    {
        _employerSchemesRepository = employerSchemesRepository;
    }

    public async Task<PayeScheme> Handle(GetAccountHistoryByPayeRefQuery query, CancellationToken cancellationToken)
    {
        return await _employerSchemesRepository.GetSchemeByRef(query.Ref);
    }
}
