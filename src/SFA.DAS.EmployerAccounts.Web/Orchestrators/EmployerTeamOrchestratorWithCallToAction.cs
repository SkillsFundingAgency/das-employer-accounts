﻿using AutoMapper;
using MediatR;
using SFA.DAS.Authorization.Services;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Models.CommitmentsV2;
using SFA.DAS.EmployerAccounts.Queries.GetSingleCohort;
using SFA.DAS.EmployerAccounts.Queries.GetApprenticeship;
using SFA.DAS.EmployerAccounts.Queries.GetReservations;
using SFA.DAS.EmployerAccounts.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using SFA.DAS.EmployerAccounts.Queries.GetVacancies;
using SFA.DAS.EmployerAccounts.Models.Recruit;
using ResourceNotFoundException = SFA.DAS.EmployerAccounts.Web.Exceptions.ResourceNotFoundException;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Web.Models;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EmployerAccounts.Web.Orchestrators
{
    public class EmployerTeamOrchestratorWithCallToAction : EmployerTeamOrchestrator
    {
        internal const string AccountContextCookieName = "sfa-das-employerapprenticeshipsservice-accountcontext";
        private readonly EmployerTeamOrchestrator _employerTeamOrchestrator;
        private readonly ICookieStorageService<AccountContext> _accountContext;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILog _logger;

        public EmployerTeamOrchestratorWithCallToAction(
            EmployerTeamOrchestrator employerTeamOrchestrator,
            IMediator mediator,
            ICurrentDateTime currentDateTime,
            IAccountApiClient accountApiClient,
            IMapper mapper,
            ICookieStorageService<AccountContext> accountContext,
            ILog logger) 
            : base(mediator, currentDateTime, accountApiClient, mapper)
        {
            _employerTeamOrchestrator = employerTeamOrchestrator;
            _accountContext = accountContext;
            _mediator = mediator;
            _mapper = mapper;
            _logger = logger;
        }

        public override async Task<OrchestratorResponse<AccountDashboardViewModel>> GetAccount(string hashedAccountId, string externalUserId)
        {
            var accountResponseTask = _employerTeamOrchestrator.GetAccount(hashedAccountId, externalUserId);

            if (TryGetAccountContext(hashedAccountId, out AccountContext accountContext))
            {
                if (accountContext.ApprenticeshipEmployerType == ApprenticeshipEmployerType.Levy)
                {
                    var levyResponse = await accountResponseTask;
                    SaveContext(levyResponse);
                    return levyResponse;
                }
            }

            // here we are either non levy or unknown caller context            
            var callToActionResponse = await GetCallToAction(hashedAccountId, externalUserId);
            var accountResponse = await accountResponseTask;
            if (accountResponse.Status == HttpStatusCode.OK)
            {
                if (callToActionResponse.Status == HttpStatusCode.OK)
                {
                    accountResponse.Data.CallToActionViewModel = callToActionResponse.Data;
                }
                else
                {
                    _logger.Error(callToActionResponse.Exception, $"An error occured whilst trying to retrieve account CallToAction: {hashedAccountId}");
                }
            }

            SaveContext(accountResponse);
            return accountResponse;
        }

        private void SaveContext(OrchestratorResponse<AccountDashboardViewModel> orchestratorResponse)
        {
            if (orchestratorResponse.Status == HttpStatusCode.OK)
            {
                _accountContext.Delete(AccountContextCookieName);

                _accountContext.Create(
                    new AccountContext
                    {
                        HashedAccountId = orchestratorResponse.Data.HashedAccountId,
                        ApprenticeshipEmployerType = orchestratorResponse.Data.ApprenticeshipEmployerType
                    }
                , AccountContextCookieName);
            }
        }

        private bool TryGetAccountContext(string hashedAccountId, out AccountContext accountContext)
        {
            if (_accountContext.Get(AccountContextCookieName) is AccountContext accountCookie)
            {
                if (accountCookie.HashedAccountId.Equals(hashedAccountId, StringComparison.InvariantCultureIgnoreCase))
                {
                    accountContext = accountCookie;
                    return true;
                }
            }

            accountContext = null;
            return false;
        }

        private async Task<OrchestratorResponse<CallToActionViewModel>> GetCallToAction(string hashedAccountId, string externalUserId)
        {
            try
            {
                var reservationsResponseTask = _mediator.SendAsync(new GetReservationsRequest
                {
                    HashedAccountId = hashedAccountId,
                    ExternalUserId = externalUserId
                });

                var apprenticeshipsResponseTask = _mediator.SendAsync(new GetApprenticeshipsRequest
                {
                    HashedAccountId = hashedAccountId,
                    ExternalUserId = externalUserId
                });

                var accountCohortResponseTask = _mediator.SendAsync(new GetSingleCohortRequest
                {
                    HashedAccountId = hashedAccountId,
                    ExternalUserId = externalUserId
                });

                var vacanciesResponseTask = _mediator.SendAsync(new GetVacanciesRequest
                {
                    HashedAccountId = hashedAccountId,
                    ExternalUserId = externalUserId
                });

                await Task.WhenAll(reservationsResponseTask, vacanciesResponseTask, apprenticeshipsResponseTask, accountCohortResponseTask).ConfigureAwait(false);

                var reservationsResponse = reservationsResponseTask.Result;
                var vacanciesResponse = vacanciesResponseTask.Result;
                var apprenticeshipsResponse = apprenticeshipsResponseTask.Result;
                var accountCohortResponse = accountCohortResponseTask.Result;

                CallToActionViewModel viewModel = null;
                var status = HttpStatusCode.OK;

                if (vacanciesResponse.HasFailed || reservationsResponse.HasFailed || accountCohortResponse.HasFailed || apprenticeshipsResponse.HasFailed)
                {
                    status = HttpStatusCode.InternalServerError;
                }
                else
                {
                    viewModel = new CallToActionViewModel
                    {
                        Reservations = reservationsResponse.Reservations?.ToList(),
                        VacanciesViewModel = new VacanciesViewModel
                        {
                            VacancyCount = vacanciesResponse.Vacancies.Count(),
                            Vacancies = _mapper.Map<IEnumerable<Vacancy>, IEnumerable<VacancyViewModel>>(vacanciesResponse.Vacancies)
                        },
                        Apprenticeships = _mapper.Map<IEnumerable<Apprenticeship>, IEnumerable<ApprenticeshipViewModel>>(apprenticeshipsResponse?.Apprenticeships),
                        Cohorts = new List<CohortViewModel>
                        {
                            _mapper.Map<Cohort, CohortViewModel>(accountCohortResponse.Cohort)
                        }
                    };
                }

                return new OrchestratorResponse<CallToActionViewModel>
                {
                    Status = status,
                    Data = viewModel
                };
            }
            catch (Exception ex)
            {
                return new OrchestratorResponse<CallToActionViewModel>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Exception = ex
                };
            }
        }
    }
}