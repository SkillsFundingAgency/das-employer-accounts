﻿using System;
using System.Threading.Tasks;
using SFA.DAS.Authorization.EmployerUserRoles.Options;
using SFA.DAS.Authorization.Services;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.HashingService;
using SFA.DAS.EmployerFinance.Web.ViewModels.Transfers;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.Authorization.Features.Services;
using SFA.DAS.Authorization.EmployerFeatures.Models;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EmployerFinance.Extensions;

namespace SFA.DAS.EmployerFinance.Web.Orchestrators
{
    public class TransfersOrchestrator
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IHashingService _hashingService;
        private readonly IManageApprenticeshipsService _manageApprenticeshipsService;
        private readonly IAccountApiClient _accountApiClient;
        private readonly IFeatureTogglesService<EmployerFeatureToggle> _featureTogglesService;

        protected TransfersOrchestrator()
        {

        }

        public TransfersOrchestrator(
            IAuthorizationService authorizationService,
            IHashingService hashingService,
            IManageApprenticeshipsService manageApprenticeshipsService,
            IAccountApiClient accountApiClient,
            IFeatureTogglesService<EmployerFeatureToggle> featureTogglesService)
        {
            _authorizationService = authorizationService;
            _hashingService = hashingService;
            _manageApprenticeshipsService = manageApprenticeshipsService;
            _accountApiClient = accountApiClient;
            _featureTogglesService = featureTogglesService;
        }

        public async Task<OrchestratorResponse<IndexViewModel>> GetIndexViewModel(string hashedAccountId)
        {
            var accountId = _hashingService.DecodeValue(hashedAccountId);
            var indexTask = _manageApprenticeshipsService.GetIndex(accountId);
            var accountDetail = _accountApiClient.GetAccount(hashedAccountId);

            var renderCreateTransfersPledgeButtonTask = _authorizationService.IsAuthorizedAsync(EmployerUserRole.OwnerOrTransactor);
            var renderApplicationListButton = _featureTogglesService.GetFeatureToggle("ApplicationList");

            await Task.WhenAll(indexTask, renderCreateTransfersPledgeButtonTask, accountDetail);

            Enum.TryParse(accountDetail.Result.ApprenticeshipEmployerType, true, out ApprenticeshipEmployerType employerType);

            return new OrchestratorResponse<IndexViewModel>
            {
                Data = new IndexViewModel
                {
                    CanViewPledgesSection = employerType == ApprenticeshipEmployerType.Levy,
                    PledgesCount = indexTask.Result.PledgesCount,
                    ApplicationsCount = indexTask.Result.ApplicationsCount,
                    RenderCreateTransfersPledgeButton = renderCreateTransfersPledgeButtonTask.Result,
                    RenderApplicationListButton = renderApplicationListButton.IsEnabled,
                    StartingTransferAllowance = accountDetail.Result.StartingTransferAllowance,
                    FinancialYearString = DateTime.UtcNow.ToFinancialYearString(),
                    HashedAccountID = hashedAccountId
                }
            };
        }

        public async Task<OrchestratorResponse<FinancialBreakdownViewModel>> GetFinancialBreakdownViewModel(string hashedAccountId) 
        {
            var accountId = _hashingService.DecodeValue(hashedAccountId);
            var financialBreakdownTask = await _manageApprenticeshipsService.GetFinancialBreakdown(accountId);
            var accountDetail = await _accountApiClient.GetAccount(hashedAccountId);

            return new OrchestratorResponse<FinancialBreakdownViewModel>
            {
                Data = new FinancialBreakdownViewModel
                {
                    TransferConnections = financialBreakdownTask.TransferConnections,
                    HashedAccountID = hashedAccountId,
                    AcceptedPledgeApplications = financialBreakdownTask.AcceptedPledgeApplications,
                    ApprovedPledgeApplications = financialBreakdownTask.ApprovedPledgeApplications,
                    Commitments = financialBreakdownTask.Commitments,
                    PledgeOriginatedCommitments = financialBreakdownTask.PledgeOriginatedCommitments,
                    FundsIn = financialBreakdownTask.FundsIn,
                    NumberOfMonths = financialBreakdownTask.NumberOfMonths,
                    ProjectionStartDate = financialBreakdownTask.ProjectionStartDate,
                    StartingTransferAllowance = accountDetail.StartingTransferAllowance,
                    FinancialYearString = DateTime.UtcNow.ToFinancialYearString(),
                }
            };
        }
    }
}