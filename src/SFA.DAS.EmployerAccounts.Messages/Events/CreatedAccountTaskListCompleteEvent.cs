using System;

namespace SFA.DAS.EmployerAccounts.Messages.Events
{
    public class CreatedAccountTaskListCompleteEvent
    {
        public long AccountId { get; set; }
        public string Name { get; set; }
        public Guid UserRef { get; set; }
    }
}
