using SFA.DAS.EmployerAccounts.Models.Account;

namespace SFA.DAS.EmployerAccounts.Queries.GetAccountsSinceDate;

public class GetAccountsSinceDateResponse
{
    public Accounts<AccountNameSummary> Accounts { get; set; } = new Accounts<AccountNameSummary>();
}
