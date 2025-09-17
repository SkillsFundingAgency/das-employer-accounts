using SFA.DAS.EmployerAccounts.Api.Types;
using SFA.DAS.EmployerAccounts.Models.Account;

namespace SFA.DAS.EmployerAccounts.Queries.GetAccounts
{
    public class GetAccountsResponse
    {
        public Accounts<AccountUpdates> Accounts { get; set; } = new Accounts<AccountUpdates>();
    }

    public class AccountUpdates
    {
        public string AccountName { get; set; }
        public long AccountId { get; set; }
    }
}
