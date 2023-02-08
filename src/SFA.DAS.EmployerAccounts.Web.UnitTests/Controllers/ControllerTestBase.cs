﻿using System.Linq;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Controllers;

public abstract class ControllerTestBase
{
    protected Mock<HttpRequest> HttpRequest = new();
    protected Mock<HttpContext> MockHttpContext = new();
    protected ControllerContext ControllerContext;
    protected Mock<HttpResponse> HttpResponse = new();
    protected RouteData Routes;
    protected Mock<ILog> Logger;
    protected Mock<IMediator> Mediator;

    public virtual void Arrange(string redirectUrl = "http://localhost/testpost")
    {
        Logger = new Mock<ILog>();
        Mediator = new Mock<IMediator>();

        Routes = new RouteData();

        MockHttpContext.Setup(x => x.Request.Host).Returns(new HostString("test.local"));
        MockHttpContext.Setup(x => x.Request.Scheme).Returns("http");
        MockHttpContext.Setup(x => x.Request.PathBase).Returns("/");
        MockHttpContext.Setup(x => x.Connection.RemoteIpAddress).Returns(IPAddress.Parse("123.123.123.123"));
        MockHttpContext.Setup(c => c.Request).Returns(HttpRequest.Object);
        MockHttpContext.Setup(c => c.Response).Returns(HttpResponse.Object);

        ControllerContext = new ControllerContext()
        {
            HttpContext = MockHttpContext.Object,
            RouteData = Routes,
        };
    }

    protected void AddEmptyUserToContext()
    {
        var identity = new ClaimsIdentity();

        MockHttpContext.Setup(c => c.User).Returns(new ClaimsPrincipal(identity));
    }

    protected void AddUserToContext(params Claim[] claims)
    {
        AddUserToContext("USER_ID", "my@local.com", "test name", claims);
    }

    protected void AddUserToContext(string id = "USER_ID", string email = "my@local.com", string name = "test name", params Claim[] claims)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Email, email),
            new Claim("sub", id),
        });

        if (claims != null && claims.Any())
        {
            identity.AddClaims(claims);
        }

        var principal = new ClaimsPrincipal(identity);

        MockHttpContext.Setup(c => c.User).Returns(principal);
    }

    protected void AddUserToContext(string id, string email, string givenName, string familyName)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, $"{givenName} {familyName}"),
            new Claim(ClaimTypes.Email, email),
            new Claim(ControllerConstants.EmailClaimKeyName, email),
            new Claim(DasClaimTypes.GivenName, givenName),
            new Claim(DasClaimTypes.FamilyName, familyName),
            new Claim(ControllerConstants.UserRefClaimKeyName, id),
            new Claim("sub", id),
        });

        var principal = new ClaimsPrincipal(identity);
        MockHttpContext.Setup(c => c.User).Returns(principal);
    }
}