﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.EAS.Finance.Api.Types
{
    public class TransactionsViewModel : List<TransactionViewModel>
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public bool HasPreviousTransactions { get; set; }
        public string PreviousMonthUri { get; set; }
    }
}
