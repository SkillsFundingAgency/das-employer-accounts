using System.Net;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Audit.MessageBuilders;
using SFA.DAS.EmployerAccounts.Audit.Types;
using SFA.DAS.EmployerAccounts.Interfaces;

namespace SFA.DAS.EmployerAccounts.UnitTests.Audit.MessageBuilders;

public class WhenBuildingAnAuditMessage
{
    private const string IpString = "127.1.1.1";

    [Test, AutoData]
    public async Task Then_Do_Not_Build_User_If_It_Already_Exists(AuditMessage auditMessage, Actor actor)
    {
        Mock<IHttpContextAccessor> httpContextAccessorMock = new();
        Mock<IUserRepository> userRepositoryMock = new();

        var httpContext = new DefaultHttpContext();
        httpContext.Features.Set<IHttpConnectionFeature>(new HttpConnectionFeature
        {
            RemoteIpAddress = IPAddress.Parse(IpString)
        });
        httpContextAccessorMock.Setup(c => c.HttpContext).Returns(httpContext);

        auditMessage.ChangedBy = actor;

        ChangedByMessageBuilder sut = new(httpContextAccessorMock.Object, userRepositoryMock.Object);

        await sut.Build(auditMessage);

        auditMessage.ChangedBy.Should().Be(actor);
        auditMessage.ChangedBy.OriginIpAddress.Should().Be(IpString);
    }
}
