﻿using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EAS.Application.Queries.GetFinancialStatistics;
using SFA.DAS.EAS.Application.Services.EmployerAccountsApi;
using SFA.DAS.EmployerFinance.Services;

namespace SFA.DAS.EAS.Account.Api.Orchestrators
{
    public class StatisticsOrchestrator
    {
        private readonly IMediator _mediator;
        private readonly IEmployerAccountsApiService _employerAccountsApiService;
        private readonly IEmployerFinanceApiService _employerFinanceApiService;

        public StatisticsOrchestrator(IMediator mediator, IEmployerAccountsApiService employerAccountsApiService, IEmployerFinanceApiService employerFinanceApiService)
        {
            _mediator = mediator;
            _employerAccountsApiService = employerAccountsApiService;
            _employerFinanceApiService = employerFinanceApiService;
        }

        public virtual async Task<StatisticsViewModel> Get()
        {
            var getAccountStatisticsTask = _employerAccountsApiService.GetStatistics();
            var financialStatisticsQueryTask = _employerFinanceApiService.GetStatistics(); //_mediator.SendAsync(new GetFinancialStatisticsQuery());

            var accountStatistics = await getAccountStatisticsTask;
            return new StatisticsViewModel
            {
                TotalAccounts = accountStatistics.TotalAccounts,
                TotalAgreements = accountStatistics.TotalAgreements,
                TotalLegalEntities = accountStatistics.TotalLegalEntities,
                TotalPayeSchemes = accountStatistics.TotalPayeSchemes,
                TotalPayments = (await financialStatisticsQueryTask).TotalPayments
            };
        }
    }
}