using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Api.Controllers;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Controllers.LegalEntitiesControllerTests
{
    public abstract class LegalEntitiesControllerTests
    {
        protected LegalEntitiesController Controller;
        protected Mock<IMediator> Mediator;
        protected Mock<IUrlHelper> UrlTestHelper;
        protected Mock<ILogger<LegalEntitiesController>> Logger;

        [SetUp]
        public void Arrange()
        {
            Mediator = new Mock<IMediator>();
            UrlTestHelper = new Mock<IUrlHelper>();
            Logger = new Mock<ILogger<LegalEntitiesController>>();

            Controller = new LegalEntitiesController(Mediator.Object, Logger.Object);

            Controller.Url = UrlTestHelper.Object;

        }
    }
}
