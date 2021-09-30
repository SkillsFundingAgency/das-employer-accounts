﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.EmployerAccounts.Api.Types
{
    public class TransferConnectionWithoutEncoding
    {
        public long FundingEmployerAccountId { get; set; }
        public string FundingEmployerHashedAccountId { get; set; }
        public string FundingEmployerPublicHashedAccountId { get; set; }
        public string FundingEmployerAccountName { get; set; }
    }
}
