﻿using System;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Data;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.NLog.Logger;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerAccounts.Commands.AccountLevyStatus
{
    public class AccountLevyStatusCommandHandler : AsyncRequestHandler<AccountLevyStatusCommand>
    {
        private readonly IEmployerAccountRepository _accountRepositoryObject;
        private readonly ILog _logger;
        private readonly IEventPublisher _eventPublisher;

        public AccountLevyStatusCommandHandler(
            IEmployerAccountRepository accountRepositoryObject,
            ILog logger,
            IEventPublisher eventPublisher)
        {
            _accountRepositoryObject = accountRepositoryObject;
            _logger = logger;
            _eventPublisher = eventPublisher;
        }

        protected override async Task HandleCore(AccountLevyStatusCommand command)
        {
            var account = await _accountRepositoryObject.GetAccountById(command.AccountId);

            // 1. Prevent setting status to same status
            // 2. Prevent status being changed from Levy to any other status
            // 3. Prevent status being changed to Unknown
            if ((ApprenticeshipEmployerType) account.ApprenticeshipEmployerType == command.ApprenticeshipEmployerType ||
                (ApprenticeshipEmployerType) account.ApprenticeshipEmployerType == ApprenticeshipEmployerType.Levy ||
                command.ApprenticeshipEmployerType == ApprenticeshipEmployerType.Unknown)
            {
                return;
            }

            _logger.Info(UpdatedStartedMessage(command));

            try
            {
                await _accountRepositoryObject.SetAccountLevyStatus(command.AccountId, command.ApprenticeshipEmployerType);

                await _eventPublisher.Publish(new ApprenticeshipEmployerTypeChangeEvent
                {
                    AccountId = command.AccountId,
                    ApprenticeshipEmployerType = command.ApprenticeshipEmployerType
                });

                _logger.Info(UpdateCompleteMessage(command));
            }
            catch (Exception ex)
            {
               _logger.Error(ex, UpdateErrorMessage(command));
            }
        }

        private string UpdatedStartedMessage(AccountLevyStatusCommand updateCommand)
        {
            return $"About to update Account with id: {updateCommand.AccountId} to {updateCommand.ApprenticeshipEmployerType} status.";
        }

        private string UpdateCompleteMessage(AccountLevyStatusCommand updateCommand)
        {
            return $"Updated Account with id: {updateCommand.AccountId} to {updateCommand.ApprenticeshipEmployerType} status.";
        }

        private string UpdateErrorMessage(AccountLevyStatusCommand updateCommand)
        {
            return $"Error updating Account with id: {updateCommand.AccountId} to {updateCommand.ApprenticeshipEmployerType} status.";
        }
    }
}