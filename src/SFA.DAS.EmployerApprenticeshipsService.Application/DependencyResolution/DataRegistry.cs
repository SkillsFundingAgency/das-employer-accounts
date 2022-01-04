﻿using System;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.Azure.Services.AppAuthentication;
using NServiceBus.Persistence;
using SFA.DAS.EAS.Domain.Configuration;
using SFA.DAS.EAS.Infrastructure.Data;
using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.NServiceBus.Features.ClientOutbox.Data;
using SFA.DAS.UnitOfWork.Context;
using StructureMap;

namespace SFA.DAS.EAS.Application.DependencyResolution
{
    public class DataRegistry : Registry
    {
        private const string AzureResource = "https://database.windows.net/";

        public DataRegistry()
        {
            var environmentName = ConfigurationManager.AppSettings["EnvironmentName"];

            For<DbConnection>().Use($"Build DbConnection", c =>
            {
                var azureServiceTokenProvider = new AzureServiceTokenProvider();

                return environmentName.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase)
                    ? new SqlConnection(GetEmployerAccountsConnectionString(c))
                    : new SqlConnection
                    {
                        ConnectionString = GetEmployerAccountsConnectionString(c),
                        AccessToken = azureServiceTokenProvider.GetAccessTokenAsync(AzureResource).Result
                    };
            });

            For<EmployerAccountsDbContext>().Use(c => GetEmployerAccountsDbContext(c));
            For<EmployerFinanceDbContext>().Use(c => GetEmployerFinanceDbContext(c));
        }

        private EmployerAccountsDbContext GetEmployerAccountsDbContext(IContext context)
        {
            var unitOfWorkContext = context.GetInstance<IUnitOfWorkContext>();
            var clientSession = unitOfWorkContext.Find<IClientOutboxTransaction>();
            var serverSession = unitOfWorkContext.Find<SynchronizedStorageSession>();
            var sqlSession = clientSession?.GetSqlSession() ?? serverSession.GetSqlSession();

            return new EmployerAccountsDbContext(sqlSession.Connection, sqlSession.Transaction);
        }

        private EmployerFinanceDbContext GetEmployerFinanceDbContext(IContext c)
        {
            var environmentName = ConfigurationManager.AppSettings["EnvironmentName"];

            var azureServiceTokenProvider = new AzureServiceTokenProvider();

            var connection = environmentName.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase)
                ? new SqlConnection(GetEmployerFinanceConnectionString(c))
                : new SqlConnection
                {
                    ConnectionString = GetEmployerFinanceConnectionString(c),
                    AccessToken = azureServiceTokenProvider.GetAccessTokenAsync(AzureResource).Result
                };

            return new EmployerFinanceDbContext(connection);
        }

        private string GetEmployerAccountsConnectionString(IContext context)
        {
            return context.GetInstance<EmployerApprenticeshipsServiceConfiguration>().DatabaseConnectionString;
        }

        private string GetEmployerFinanceConnectionString(IContext context)
        {
            return context.GetInstance<LevyDeclarationProviderConfiguration>().DatabaseConnectionString;
        }
    }
}