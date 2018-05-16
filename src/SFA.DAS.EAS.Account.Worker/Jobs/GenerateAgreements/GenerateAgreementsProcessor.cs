﻿using System;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EAS.Account.Worker.IdProcessor;
using SFA.DAS.EAS.Application.Commands.CreateEmployerAgreement;
using SFA.DAS.EAS.Domain.Data.Repositories;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EAS.Account.Worker.Jobs.GenerateAgreements
{
    public class GenerateAgreementsIdProcessor : IProcessor
    {
        private readonly IMediator _mediator;
        private readonly ILegalEntityRepository _legalEntityRepository;
        private readonly ILog _log;

        public GenerateAgreementsIdProcessor(
            IMediator mediator,
            ILegalEntityRepository legalEntityRepository,
            ILog log)
        {
            _legalEntityRepository = legalEntityRepository;
            _log = log;
            _mediator = mediator;
        }

        public async Task<bool> DoAsync(long legalEntityId, ProcessingContext processorContext)
        {
            if(!processorContext.TryGet<int>(Constants.ProcessingContextValues.LatestTemplateId, out int latestAgreementId))
            {
                _log.Warn($"Terminating processor {nameof(GenerateAgreementsIdProcessor)} early because there is no value for {Constants.ProcessingContextValues.LatestTemplateId}");
                return false;
            }

            var accountsLinkedToLegalEntity = await _legalEntityRepository.GetAccountsLinkedToLegalEntityWithoutSpecificAgreement(legalEntityId, latestAgreementId);

            foreach (var accountId in accountsLinkedToLegalEntity)
            {
                var response = await _mediator.SendAsync(new CreateEmployerAgreementCommand
                {
                    LatestTemplateId = latestAgreementId,
                    AccountId = accountId,
                    LegalEntityId = legalEntityId
                });
            }

            return true;
        }

        public Task<bool> InspectFailedAsync(long id, Exception exception, ProcessingContext processorContext)
        {
            return Task.FromResult(false);
        }
    }
}
