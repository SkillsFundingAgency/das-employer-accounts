﻿using System;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Messages.Commands;
using SFA.DAS.EmployerFinance.Types.Models;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EmployerFinance.MessageHandlers.CommandHandlers
{
    public class DraftExpireAccountFundsCommandHandler : IHandleMessages<DraftExpireAccountFundsCommand>
    {
        private readonly ICurrentDateTime _currentDateTime;
        private readonly ILevyFundsInRepository _levyFundsInRepository;
        private readonly IPaymentFundsOutRepository _paymentFundsOutRepository;
        private readonly IExpiredFunds _expiredFunds;
        private readonly IExpiredFundsRepository _expiredFundsRepository;
        private readonly ILog _logger;
        private readonly EmployerFinanceConfiguration _configuration;

        public DraftExpireAccountFundsCommandHandler(
            ICurrentDateTime currentDateTime,
            ILevyFundsInRepository levyFundsInRepository,
            IPaymentFundsOutRepository paymentFundsOutRepository,
            IExpiredFunds expiredFunds,
            IExpiredFundsRepository expiredFundsRepository,
            ILog logger,
            EmployerFinanceConfiguration configuration)
        {
            _currentDateTime = currentDateTime;
            _levyFundsInRepository = levyFundsInRepository;
            _paymentFundsOutRepository = paymentFundsOutRepository;
            _expiredFunds = expiredFunds;
            _expiredFundsRepository = expiredFundsRepository;
            _logger = logger;
            _configuration = configuration;
        }
        public async Task Handle(DraftExpireAccountFundsCommand message, IMessageHandlerContext context)
        {
            _logger.Info($"DRAFT: Expiring funds for account ID '{message.AccountId}' with expiry period '{_configuration.FundsExpiryPeriod}'");

            var now = _currentDateTime.Now;
            var fundsIn = await _levyFundsInRepository.GetLevyFundsIn(message.AccountId);
            var fundsOut = await _paymentFundsOutRepository.GetPaymentFundsOut(message.AccountId);
            var existingExpiredFunds = await _expiredFundsRepository.GetDraft(message.AccountId);

            if(message.DateTo != null)
            {
                fundsIn = fundsIn.Where(c =>
                    new DateTime(c.CalendarPeriodYear, c.CalendarPeriodMonth, 1) <=
                    new DateTime(message.DateTo.Value.Year, message.DateTo.Value.Month, 1)).ToList();
                
                fundsOut = fundsOut.Where(c =>
                    new DateTime(c.CalendarPeriodYear, c.CalendarPeriodMonth, 1) <=
                    new DateTime(message.DateTo.Value.Year, message.DateTo.Value.Month, 1)).ToList();
            }
            

            var expiredFunds = _expiredFunds.GetExpiredFunds(
                fundsIn.ToCalendarPeriodDictionary(),
                fundsOut.ToCalendarPeriodDictionary(),
                existingExpiredFunds.ToCalendarPeriodDictionary(),
                _configuration.FundsExpiryPeriod,
                now);

            await _expiredFundsRepository.CreateDraft(message.AccountId, expiredFunds.ToExpiredFundsList(), now);

            _logger.Info($"DRAFT: Expired '{expiredFunds.Count}' month(s) of funds for account ID '{message.AccountId}' with expiry period '{_configuration.FundsExpiryPeriod}'");
        }
    }
}