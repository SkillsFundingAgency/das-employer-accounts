using System.Threading;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.Account;

namespace SFA.DAS.EmployerAccounts.Queries.GetUserAccounts;

public class GetUserAccountsQueryHandler(IUserAccountRepository userAcountRepository) : IRequestHandler<GetUserAccountsQuery, GetUserAccountsQueryResponse>
{
    public async Task<GetUserAccountsQueryResponse> Handle(GetUserAccountsQuery message, CancellationToken cancellationToken)
    {
        //TODO add validator.
        var userRef = message.UserRef;
        Accounts<Account> accounts;

        if (!string.IsNullOrEmpty(userRef))
        {
            accounts = await userAcountRepository.GetAccountsByUserRef(userRef);
        }
        else
        {
            accounts = await userAcountRepository.GetAccounts();
        }

        return new GetUserAccountsQueryResponse { Accounts = accounts };
    }
}