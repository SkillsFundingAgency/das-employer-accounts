﻿using System.Data.Common;
using System.Net;
using NServiceBus;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Extensions;
using SFA.DAS.NServiceBus;
using SFA.DAS.NServiceBus.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.NLog;
using SFA.DAS.NServiceBus.SqlServer;
using SFA.DAS.NServiceBus.StructureMap;
using SFA.DAS.UnitOfWork.NServiceBus;
using StructureMap;

namespace SFA.DAS.EmployerAccounts.Api
{
    public class ServiceBusEndPointConfigureAndRun
    {
        private readonly IContainer _container;
        private IEndpointInstance _endpoint;

        public ServiceBusEndPointConfigureAndRun(IContainer container)
        {
            _container = container;
        }

        public void ConfigureAndStartServiceBusEndpoint()
        {
            var endpointConfiguration = new EndpointConfiguration("SFA.DAS.EmployerAccounts.Api")
                .UseAzureServiceBusTransport(() => _container.GetInstance<EmployerAccountsConfiguration>().ServiceBusConnectionString, _container)
                .UseErrorQueue()
                .UseInstallers()
                .UseLicense(WebUtility.HtmlDecode(_container.GetInstance<EmployerAccountsConfiguration>().NServiceBusLicense))
                .UseSqlServerPersistence(() => _container.GetInstance<DbConnection>())
                .UseNewtonsoftJsonSerializer()
                .UseNLogFactory()
                .UseOutbox()
                .UseStructureMapBuilder(_container)
                .UseUnitOfWork();

            _endpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();

            _container.Configure(c =>
            {
                c.For<IMessageSession>().Use(_endpoint);
            });
        }

        public void StopServiceBusEndpoint()
        {
            _endpoint?.Stop().GetAwaiter().GetResult();
        }
    }
}