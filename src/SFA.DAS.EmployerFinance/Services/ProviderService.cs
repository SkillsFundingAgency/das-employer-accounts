﻿using System;
using System.Threading.Tasks;
using SFA.DAS.EmployerFinance.Models.Payments;

namespace SFA.DAS.EmployerFinance.Services
{
    public class ProviderService : IProviderService
    {

        private readonly IApprenticeshipInfoServiceWrapper _apprenticeshipInfoServiceWrapper;
        private readonly IPaymentService _paymentService;

        public ProviderService(IApprenticeshipInfoServiceWrapper apprenticeshipInfoServiceWrapper, IPaymentService paymentService)
        {
            _apprenticeshipInfoServiceWrapper = apprenticeshipInfoServiceWrapper;
            _paymentService = paymentService;
        }
        public virtual string GetProvider(long ukprn)
        {
            var providerFromDb = _paymentService.GetProvider((Int16)ukprn);
            var providerName = providerFromDb.Result.ProviderName;
           return providerName;
        }
    }
}