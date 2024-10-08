using System;

namespace SFA.DAS.EmployerAccounts.Api.Types
{
    public class AccountHistory
    {
        public long AccountId { get; set; }
        public DateTime AddedDate { get; set; }
        public DateTime? RemovedDate { get; set; }
    }
}