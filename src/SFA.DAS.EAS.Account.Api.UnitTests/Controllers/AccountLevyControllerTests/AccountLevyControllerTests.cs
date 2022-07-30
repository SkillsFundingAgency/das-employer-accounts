﻿using System;
using System.Linq;
using System.Reflection;
using AutoMapper;
using Castle.Core.Internal;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Account.Api.Controllers;
using SFA.DAS.EAS.Account.Api.Orchestrators;
using SFA.DAS.EAS.Application.Services.EmployerAccountsApi;
using SFA.DAS.EAS.Domain.Interfaces;
using SFA.DAS.NLog.Logger;
using SFA.DAS.HashingService;
using SFA.DAS.EmployerFinance.Services;

namespace SFA.DAS.EAS.Account.Api.UnitTests.Controllers.AccountLevyControllerTests
{
    public abstract class AccountLevyControllerTests
    {
        protected AccountLevyController Controller;
        protected Mock<IMediator> Mediator;
        protected Mock<ILog> Logger;
        protected IMapper Mapper;
        protected Mock<IHashingService> HashingService;
        protected Mock<IEmployerAccountsApiService> ApiService;
        protected Mock<IEmployerFinanceApiService> FinanceApiService;

        [SetUp]
        public void Arrange()
        {
            Mediator = new Mock<IMediator>();
            Logger = new Mock<ILog>();
            HashingService = new Mock<IHashingService>();
            ApiService = new Mock<IEmployerAccountsApiService>();
            FinanceApiService = new Mock<IEmployerFinanceApiService>();
            Mapper = ConfigureMapper();
            var orchestrator = new AccountsOrchestrator(Mediator.Object, Logger.Object, Mapper, HashingService.Object, ApiService.Object, FinanceApiService.Object);
            Controller = new AccountLevyController(orchestrator);
        }

        private IMapper ConfigureMapper()
        {
            var profiles = Assembly.Load($"SFA.DAS.EAS.Account.Api")
                .GetTypes()
                .Where(t => typeof(Profile).IsAssignableFrom(t))
                .Select(t => (Profile)Activator.CreateInstance(t));

            var config = new MapperConfiguration(c =>
            {
                profiles.ForEach(c.AddProfile);
            });

            return config.CreateMapper();
        }
    }
}

