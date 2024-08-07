using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Api.Controllers;
using SFA.DAS.EmployerAccounts.Queries.GetUserByRef;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.Api.UnitTests.Controllers.UserControllerTests;

public class WhenICallTheGetUserByRefEndPoint
{
    [Test, MoqAutoData]
    public async Task ThenShouldReturnAUser(
        Mock<IMediator> mediator,
        GetUserByRefQuery query,
        GetUserByRefResponse response)
    {
        var sut = new UserController(mediator.Object, Mock.Of<ILogger<UserController>>());
        mediator.Setup(m => m.Send(It.Is<GetUserByRefQuery>(x=> x.UserRef.Equals(query.UserRef)), It.IsAny<CancellationToken>())).ReturnsAsync(response);
        
        var result = await sut.GetByRef(query.UserRef) as OkObjectResult;

        result.Should().NotBeNull();
        result.Value.Should().Be(response.User);
    }
}