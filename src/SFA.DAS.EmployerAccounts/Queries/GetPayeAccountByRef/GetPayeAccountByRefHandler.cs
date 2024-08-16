using System.Threading;
using SFA.DAS.EmployerAccounts.Data.Contracts;

namespace SFA.DAS.EmployerAccounts.Queries.GetPayeAccountByRef;
public class GetPayeAccountByRefHandler : IRequestHandler<GetPayeAccountByRefQuery, GetPayeAccountByRefResponse>
{
    private readonly IPayeRepository _payeRepository;

    public GetPayeAccountByRefHandler(IPayeRepository payeRepository)
    {
        _payeRepository = payeRepository;
    }

    public async Task<GetPayeAccountByRefResponse> Handle(GetPayeAccountByRefQuery query, CancellationToken cancellationToken)
    {
        return await _payeRepository.GetPayeAccountByRef(query.Ref, cancellationToken);
    }
}
