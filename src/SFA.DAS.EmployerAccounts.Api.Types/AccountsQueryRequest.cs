using System.Collections.Generic;

namespace SFA.DAS.EmployerAccounts.Api.Types
{
    public class AccountsQueryRequest
    {
        public AccountsQueryFilter Filter { get; set; }
        public List<string> Select { get; set; } = new List<string>();
        public List<string> Include { get; set; } = new List<string>();
    }

    public class AccountsQueryFilter
    {
        public List<long> AccountIds { get; set; } = new List<long>();
    }
}
