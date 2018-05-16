﻿using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.EAS.Domain.Configuration;
using SFA.DAS.EAS.Domain.Data.Repositories;
using SFA.DAS.Sql.Client;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EAS.Infrastructure.Data
{
    public class LegalEntityRepository : BaseRepository, ILegalEntityRepository
    {
        public LegalEntityRepository(EmployerApprenticeshipsServiceConfiguration configuration, ILog logger)
            : base(configuration.DatabaseConnectionString, logger)
        {
        }

        public async Task<long[]> GetLegalEntitiesWithoutSpecificAgreement(long firstId, int count, int agreementId)
        {
            var result = await WithConnection(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@firstId", firstId, DbType.Int64);
                parameters.Add("@count", count, DbType.Int32);
                parameters.Add("@agreementId", agreementId, DbType.Int64);

                return await c.QueryAsync<long>(
                    sql: "[employer_account].[GetLegalEntities_WithoutSpecificAgreement]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure);

            });

            return result.ToArray();
        }

        public async Task<long[]> GetAccountsLinkedToLegalEntity(long legalEntityId)
        {
            var result = await WithConnection(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@legalEntityId", legalEntityId, DbType.Int64);

                return await c.QueryAsync<long>(
                    sql: "[employer_account].[GetAccountsLinkedToLegalEntity]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure);
            });

            return result.ToArray();
        }

        public async Task<long[]> GetAccountsLinkedToLegalEntityWithoutSpecificAgreement(long legalEntityId, int templateId)
        {
            var result = await WithConnection(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@legalEntityId", legalEntityId, DbType.Int64);
                parameters.Add("@@withoutAgreementVersion", templateId, DbType.Int32);

                return await c.QueryAsync<long>(
                    sql: "[employer_account].[GetAccountsLinkedToLegalEntity]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure);
            });

            return result.ToArray();
        }
    }
}