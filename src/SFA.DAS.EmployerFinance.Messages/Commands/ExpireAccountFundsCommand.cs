﻿using SFA.DAS.NServiceBus;

namespace SFA.DAS.EmployerFinance.Messages.Commands
{
    public class ExpireAccountFundsCommand : Command
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public long AccountId { get; set; }
    }
}
