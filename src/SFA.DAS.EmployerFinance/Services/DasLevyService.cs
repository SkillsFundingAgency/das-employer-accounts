﻿using MediatR;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Queries.AccountTransactions.GetAccountProviderPayments;
using SFA.DAS.EmployerFinance.Queries.GetAccountTransactions;
using SFA.DAS.EmployerFinance.Queries.GetPreviousTransactionsCount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Queries.AccountTransactions.GetAccountCoursePayments;
using SFA.DAS.EmployerFinance.Queries.GetAccountLevyTransactions;

namespace SFA.DAS.EmployerFinance.Services
{
    public class DasLevyService : IDasLevyService
    {
        private readonly IMediator _mediator;
        private readonly ITransactionRepository _transactionRepository;

        public DasLevyService(IMediator mediator, ITransactionRepository transactionRepository)
        {
            _mediator = mediator;
            _transactionRepository = transactionRepository;
        }

        public async Task<ICollection<TransactionLine>> GetAccountTransactionsByDateRange(long accountId, DateTime fromDate, DateTime toDate)
        {
            var result = await _mediator.SendAsync(new GetAccountTransactionsRequest
            {
                AccountId = accountId,
                FromDate = fromDate,
                ToDate = toDate
            });

            return result.TransactionLines;
        }

        public async Task<ICollection<T>> GetAccountProviderPaymentsByDateRange<T>(
            long accountId, long ukprn, DateTime fromDate, DateTime toDate) where T : TransactionLine
        {
            var result = await _mediator.SendAsync(new GetAccountProviderPaymentsByDateRangeQuery
            {
                AccountId = accountId,
                UkPrn = ukprn,
                FromDate = fromDate,
                ToDate = toDate
            });

            return result?.Transactions?.OfType<T>().ToList() ?? new List<T>();
        }
		 public async Task<ICollection<T>> GetAccountCoursePaymentsByDateRange<T>(
            long accountId, long ukprn, string courseName, int? courseLevel, int? pathwayCode, DateTime fromDate,
            DateTime toDate) where T : TransactionLine
        {
            var result = await _mediator.SendAsync(new GetAccountCoursePaymentsQuery
            {
                AccountId = accountId,
                UkPrn = ukprn,
                CourseName = courseName,
                CourseLevel = courseLevel,
                PathwayCode = pathwayCode,
                FromDate = fromDate,
                ToDate = toDate
            });

            return result?.Transactions?.OfType<T>().ToList() ?? new List<T>();
        }

        public Task<string> GetProviderName(long ukprn, long accountId, string periodEnd)
        {
            return _transactionRepository.GetProviderName(ukprn, accountId, periodEnd);
        }

        public async Task<int> GetPreviousAccountTransaction(long accountId, DateTime fromDate)
        {
            var result = await _mediator.SendAsync(new GetPreviousTransactionsCountRequest
            {
                AccountId = accountId,
                FromDate = fromDate
            });

            return result.Count;
        }

        public async Task<ICollection<T>> GetAccountLevyTransactionsByDateRange<T>(long accountId, DateTime fromDate, DateTime toDate) where T : TransactionLine
        {
            var result = await _mediator.SendAsync(new GetAccountLevyTransactionsQuery
            {
                AccountId = accountId,
                FromDate = fromDate,
                ToDate = toDate
            });

            return result?.Transactions?.OfType<T>().ToList() ?? new List<T>();
        }

    }
}
