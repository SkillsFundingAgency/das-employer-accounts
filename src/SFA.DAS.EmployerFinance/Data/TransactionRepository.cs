﻿using AutoMapper;
using Dapper;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Models.Transfers;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Sql.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerFinance.Services;

namespace SFA.DAS.EmployerFinance.Data
{
    public class TransactionRepository : BaseRepository, ITransactionRepository
    {
        private readonly IMapper _mapper;
        private readonly Lazy<EmployerFinanceDbContext> _db;

        public TransactionRepository(EmployerFinanceConfiguration configuration, IMapper mapper, ILog logger, Lazy<EmployerFinanceDbContext> db)
            : base(configuration.DatabaseConnectionString, logger)
        {
            _mapper = mapper;
            _db = db;
        }

        public async Task CreateTransferTransactions(IEnumerable<TransferTransactionLine> transactions)
        {
            var transactionTable = CreateTransferTransactionDataTable(transactions);
            var parameters = new DynamicParameters();

            parameters.Add("@transferTransactions", transactionTable.AsTableValuedParameter("[employer_financial].[TransferTransactionsTable]"));

            await _db.Value.Database.Connection.ExecuteAsync(
                sql: "[employer_financial].[CreateAccountTransferTransactions]",
                param: parameters,
                transaction: _db.Value.Database.CurrentTransaction.UnderlyingTransaction,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> GetPreviousTransactionsCount(long accountId, DateTime fromDate)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@accountId", accountId, DbType.Int64);
            parameters.Add("@fromDate", new DateTime(fromDate.Year, fromDate.Month, fromDate.Day), DbType.DateTime);

            try
            {
                return await  _db.Value.Database.Connection.ExecuteScalarAsync<int>(
                    sql: "[employer_financial].[GetPreviousTransactionsCount]",
                    param: parameters,
                    transaction: _db.Value.Database.CurrentTransaction.UnderlyingTransaction,
                    commandType: CommandType.StoredProcedure);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<List<TransactionLine>> GetAccountTransactionsByDateRange(long accountId, DateTime fromDate, DateTime toDate)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@accountId", accountId, DbType.Int64);
            parameters.Add("@fromDate", new DateTime(fromDate.Year, fromDate.Month, fromDate.Day), DbType.DateTime);
            parameters.Add("@toDate", new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 59), DbType.DateTime);

            var result = await _db.Value.Database.Connection.QueryAsync<TransactionEntity>(
                sql: "[employer_financial].[GetTransactionLines_ByAccountId]",
                param: parameters,
                transaction: _db.Value.Database.CurrentTransaction.UnderlyingTransaction,
                commandType: CommandType.StoredProcedure);

            return MapTransactions(result);
        }

        private static DataTable CreateTransferTransactionDataTable(IEnumerable<TransferTransactionLine> transactions)
        {
            var table = new DataTable();

            table.Columns.AddRange(new[]
            {
                new DataColumn("AccountId", typeof(long)),
                new DataColumn("SenderAccountId", typeof(long)),
                new DataColumn("SenderAccountName", typeof(string)),
                new DataColumn("ReceiverAccountId", typeof(long)),
                new DataColumn("ReceiverAccountName", typeof(string)),
                new DataColumn("PeriodEnd", typeof(string)),
                new DataColumn("Amount", typeof(decimal)),
                new DataColumn("TransactionType", typeof(short)),
                new DataColumn("TransactionDate", typeof(DateTime)),
            });

            foreach (var transaction in transactions)
            {
                table.Rows.Add(
                    transaction.AccountId,
                    transaction.SenderAccountId,
                    transaction.SenderAccountName,
                    transaction.ReceiverAccountId,
                    transaction.ReceiverAccountName,
                    transaction.PeriodEnd,
                    transaction.Amount,
                    (short)TransactionItemType.Transfer,
                    transaction.TransactionDate);
            }

            table.AcceptChanges();

            return table;
        }

        public async Task<List<TransactionLine>> GetAccountTransactionByProviderAndDateRange(long accountId, long ukprn, DateTime fromDate, DateTime toDate)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@accountId", accountId, DbType.Int64);
            parameters.Add("@ukprn", ukprn, DbType.Int64);
            parameters.Add("@fromDate", new DateTime(fromDate.Year, fromDate.Month, fromDate.Day), DbType.DateTime);
            parameters.Add("@toDate", new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 59), DbType.DateTime);

            var result = await _db.Value.Database.Connection.QueryAsync<TransactionEntity>(
                sql: "[employer_financial].[GetPaymentDetail_ByAccountProviderAndDateRange]",
                param: parameters,
                transaction: _db.Value.Database.CurrentTransaction.UnderlyingTransaction,
                commandType: CommandType.StoredProcedure);

            return MapTransactions(result);
        }

        private List<TransactionLine> MapTransactions(IEnumerable<TransactionEntity> transactionEntities)
        {
            var transactions = new List<TransactionLine>();

            foreach (var entity in transactionEntities)
            {
                switch (entity.TransactionType)
                {
                    case TransactionItemType.Declaration:
                    case TransactionItemType.TopUp:
                        transactions.Add(_mapper.Map<LevyDeclarationTransactionLine>(entity));
                        break;

                    case TransactionItemType.Payment:
                        transactions.Add(_mapper.Map<PaymentTransactionLine>(entity));
                        break;

                    case TransactionItemType.Transfer:
                        transactions.Add(_mapper.Map<TransferTransactionLine>(entity));
                        break;

                    case TransactionItemType.Unknown:
                        transactions.Add(_mapper.Map<TransactionLine>(entity));
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return transactions;
        }

        public async Task<List<TransactionLine>> GetAccountCoursePaymentsByDateRange(long accountId, long ukprn, string courseName, int? courseLevel, int? pathwayCode, DateTime fromDate, DateTime toDate)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@accountId", accountId, DbType.Int64);
            parameters.Add("@ukprn", ukprn, DbType.Int64);
            parameters.Add("@courseName", courseName, DbType.String);
            parameters.Add("@courseLevel", courseLevel, DbType.Int32);
            parameters.Add("@pathwayCode", pathwayCode, DbType.Int32);
            parameters.Add("@fromDate", new DateTime(fromDate.Year, fromDate.Month, fromDate.Day), DbType.DateTime);
            parameters.Add("@toDate", new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 59), DbType.DateTime);

            var result = await _db.Value.Database.Connection.QueryAsync<TransactionEntity>(
                sql: "[employer_financial].[GetPaymentDetail_ByAccountProviderCourseAndDateRange]",
                param: parameters,
                transaction: _db.Value.Database.CurrentTransaction.UnderlyingTransaction,
                commandType: CommandType.StoredProcedure);

            return MapTransactions(result);
        }
		public async Task<List<TransactionLine>> GetAccountLevyTransactionsByDateRange(long accountId, DateTime fromDate, DateTime toDate)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@accountId", accountId, DbType.Int64);
            parameters.Add("@fromDate", new DateTime(fromDate.Year, fromDate.Month, fromDate.Day), DbType.DateTime);
            parameters.Add("@toDate", new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 59), DbType.DateTime);

            var result = await _db.Value.Database.Connection.QueryAsync<TransactionEntity>(
                sql: "[employer_financial].[GetLevyDetail_ByAccountIdAndDateRange]",
                param: parameters,
                transaction: _db.Value.Database.CurrentTransaction.UnderlyingTransaction,
                commandType: CommandType.StoredProcedure);

            return MapTransactions(result);
        }
		

        public async Task<string> GetProviderName(long ukprn, long accountId, string periodEnd)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@ukprn", ukprn, DbType.Int64);
            parameters.Add("@accountId", accountId, DbType.Int64);
            parameters.Add("@periodEnd", periodEnd, DbType.String);

            return await _db.Value.Database.Connection.ExecuteScalarAsync<string>(
                sql: "[employer_financial].[GetProviderName]",
                param: parameters,
                transaction: _db.Value.Database.CurrentTransaction.UnderlyingTransaction,
                commandType: CommandType.StoredProcedure);
            
        }
        public async Task<List<TransactionDownloadLine>> GetAllTransactionDetailsForAccountByDate(long accountId, DateTime fromDate, DateTime toDate)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@AccountId", accountId, DbType.Int64);
            parameters.Add("@fromDate", fromDate, DbType.DateTime);
            parameters.Add("@toDate", toDate, DbType.DateTime);

            var result = await _db.Value.Database.Connection.QueryAsync<TransactionDownloadLine>(
                sql: "[employer_financial].[GetAllTransactionDetailsForAccountByDate]",
                param: parameters,
                transaction: _db.Value.Database.CurrentTransaction.UnderlyingTransaction,
                commandType: CommandType.StoredProcedure);

            var hmrcDateService = new HmrcDateService();
            var transactionDownloadLines = result as TransactionDownloadLine[] ?? result.ToArray();

            foreach (var res in transactionDownloadLines)
            {
                if (!string.IsNullOrEmpty(res.PayrollYear) && res.PayrollMonth != 0)
                {
                    res.PeriodEnd = hmrcDateService.GetDateFromPayrollYearMonth(res.PayrollYear, res.PayrollMonth).ToString("MMM yyyy");
                }
            }

            return transactionDownloadLines.OrderByDescending(txn => txn.DateCreated).ToList();
        }
    }
}