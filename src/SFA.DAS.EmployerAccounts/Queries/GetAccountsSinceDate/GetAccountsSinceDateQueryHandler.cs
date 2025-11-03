using System.Threading;
using SFA.DAS.EmployerAccounts.Data.Contracts;

namespace SFA.DAS.EmployerAccounts.Queries.GetAccountsSinceDate;

public class GetAccountsSinceDateQueryHandler : IRequestHandler<GetAccountsSinceDateQuery, GetAccountsSinceDateResponse>
{
    private readonly IEmployerAccountRepository _employerAccountRepository;
    private readonly IValidator<GetAccountsSinceDateQuery> _validator;

    public GetAccountsSinceDateQueryHandler(
        IEmployerAccountRepository employerAccountRepository,
        IValidator<GetAccountsSinceDateQuery> validator)
    {
        _employerAccountRepository = employerAccountRepository;
        _validator = validator;
    }

    public async Task<GetAccountsSinceDateResponse> Handle(GetAccountsSinceDateQuery message, CancellationToken cancellationToken)
    {
        var result = await _validator.ValidateAsync(message);

        if (!result.IsValid())
        {
            throw new InvalidRequestException(result.ValidationDictionary);
        }

        var accounts = await _employerAccountRepository.GetAccounts(message.SinceDate, message.PageNumber, message.PageSize);
        return new GetAccountsSinceDateResponse
        {
            Accounts = accounts
        };
    }
}
