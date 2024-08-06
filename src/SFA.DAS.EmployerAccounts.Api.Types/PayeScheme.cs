using System;

namespace SFA.DAS.EmployerAccounts.Api.Types
{
    public class PayeScheme 
    {
        public long AccountId { get; set; }
        public string Ref { get; set; }
        public string Name { get; set; }
        public DateTime AddedDate { get; set; }
        public DateTime? RemovedDate { get; set; }
    }
}