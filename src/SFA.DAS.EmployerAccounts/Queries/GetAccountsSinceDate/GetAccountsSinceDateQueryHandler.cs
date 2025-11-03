using System.Threading;
using SFA.DAS.EmployerAccounts.Data.Contracts;

namespace SFA.DAS.EmployerAccounts.Queries.GetAccountsSinceDate;

public class GetAccountsSinceDateQueryHandler(
    IEmployerAccountRepository employerAccountRepository,
    IValidator<GetAccountsSinceDateQuery> validator
) : IRequestHandler<GetAccountsSinceDateQuery, GetAccountsSinceDateResponse>
{
    public async Task<GetAccountsSinceDateResponse> Handle(GetAccountsSinceDateQuery message, CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(message);

        if (!result.IsValid())
        {
            throw new InvalidRequestException(result.ValidationDictionary);
        }

        var accounts = await employerAccountRepository.GetAccounts(message.SinceDate, message.PageNumber, message.PageSize);

        return new GetAccountsSinceDateResponse
        {
            Accounts = accounts
        };
    }
}
