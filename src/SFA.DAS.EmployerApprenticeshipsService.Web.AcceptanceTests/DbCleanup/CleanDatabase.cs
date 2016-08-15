﻿using System.Data;
using System.Threading.Tasks;
using Dapper;
using NLog;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Configuration;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Data;

namespace SFA.DAS.EmployerApprenticeshipsService.Web.AcceptanceTests.DbCleanup
{
    public class CleanDatabase : BaseRepository, ICleanDatabase
    {
        public CleanDatabase(EmployerApprenticeshipsServiceConfiguration configuration, ILogger logger) : base(configuration, logger)
        {
        }

        public async Task Execute()
        {
            var parameters = new DynamicParameters();
            parameters.Add("@INCLUDEUSERTABLE", 1, DbType.Int16);
            await WithConnection(async c => await c.ExecuteAsync(
                "[dbo].[Cleardown]",
                parameters,
                commandType: CommandType.StoredProcedure));

            await WithConnection(async c => await c.ExecuteAsync(
                "[dbo].[SeedDataForRoles]",
                null,
                commandType: CommandType.StoredProcedure));

        }
    }
}
