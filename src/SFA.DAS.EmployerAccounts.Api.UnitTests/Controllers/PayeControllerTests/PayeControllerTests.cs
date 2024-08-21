using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Api.Controllers;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Controllers.PayeControllerTests;

public abstract class PayeControllerTests
{
    protected PayeController Controller;
    protected Mock<IMediator> Mediator;
    protected Mock<IUrlHelper> UrlTestHelper;
    protected Mock<IEncodingService> EncodingService;

    [SetUp]
    public void Arrange()
    {
        Mediator = new Mock<IMediator>();
        EncodingService = new Mock<IEncodingService>();
        Controller = new PayeController(Mediator.Object);

        UrlTestHelper = new Mock<IUrlHelper>();
        Controller.Url = UrlTestHelper.Object;
    }
}
