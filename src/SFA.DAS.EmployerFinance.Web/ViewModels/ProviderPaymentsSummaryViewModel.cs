﻿using System;
using System.Collections.Generic;
using SFA.DAS.Common.Domain.Types;

namespace SFA.DAS.EmployerFinance.Web.ViewModels
{
    public class ProviderPaymentsSummaryViewModel
    {
        public string HashedAccountId { get; set; }
        public long UkPrn { get; set; }
        public string ProviderName { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public decimal LevyPaymentsTotal { get; set; }
        public decimal SFACoInvestmentsTotal { get; set; }
        public decimal EmployerCoInvestmentsTotal { get; set; }
        public decimal PaymentsTotal { get; set; }
        public ApprenticeshipEmployerType ApprenticeshipEmployerType { get; set; }

        public bool ShowNonCoInvesmentPaymentsTotal => LevyPaymentsTotal != 0 || ApprenticeshipEmployerType == ApprenticeshipEmployerType.Levy;

        public ICollection<CoursePaymentSummaryViewModel> CoursePayments { get; set; }
    }
}