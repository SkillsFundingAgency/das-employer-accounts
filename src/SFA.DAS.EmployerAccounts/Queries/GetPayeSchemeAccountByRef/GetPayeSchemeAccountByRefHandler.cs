using System.Threading;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.PAYE;

namespace SFA.DAS.EmployerAccounts.Queries.GetPayeSchemeAccountByRef;
public class GetPayeSchemeAccountByRefHandler : IRequestHandler<GetPayeSchemeAccountByRefQuery, PayeScheme>
{
    private readonly IEmployerSchemesRepository _employerSchemesRepository;

    public GetPayeSchemeAccountByRefHandler(IEmployerSchemesRepository employerSchemesRepository)
    {
        _employerSchemesRepository = employerSchemesRepository;
    }

    public async Task<PayeScheme> Handle(GetPayeSchemeAccountByRefQuery query, CancellationToken cancellationToken)
    {
        return await _employerSchemesRepository.GetSchemeByRef(query.Ref);
    }
}
