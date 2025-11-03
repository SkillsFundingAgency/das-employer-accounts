using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Api.Controllers;
using SFA.DAS.EmployerAccounts.Api.Orchestrators;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Controllers.AccountPayeSchemesControllerTests;

public abstract class AccountPayeSchemesControllerTests
{
    protected AccountPayeSchemesController Controller;
    protected Mock<IMediator> Mediator;
    protected Mock<ILogger<AccountsOrchestrator>> Logger;
    protected Mock<IUrlHelper> UrlTestHelper;
    protected Mock<IMapper> Mapper;

    [SetUp]
    public void Arrange()
    {
        Mediator = new Mock<IMediator>();
        Logger = new Mock<ILogger<AccountsOrchestrator>>();
        Mapper = new Mock<IMapper>();
        var orchestrator = new AccountsOrchestrator(Mediator.Object, Logger.Object, Mapper.Object);
        Controller = new AccountPayeSchemesController(orchestrator, Mock.Of<ILogger<AccountPayeSchemesController>>());

        UrlTestHelper = new Mock<IUrlHelper>();
        Controller.Url = UrlTestHelper.Object;
    }
}
