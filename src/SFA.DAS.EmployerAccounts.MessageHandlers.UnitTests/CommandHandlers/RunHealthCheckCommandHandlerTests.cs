using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.MessageHandlers.CommandHandlers;
using SFA.DAS.EmployerAccounts.Messages.Commands;

namespace SFA.DAS.EmployerAccounts.MessageHandlers.UnitTests.CommandHandlers;

[TestFixture]
public class RunHealthCheckCommandHandlerTests
{
    [Test]
    public async Task Handle_WhenHandlingCommand_ThenShouldAddMessageIdToDistributedCache()
    {
        var fixture = new RunHealthCheckCommandHandlerTestsFixture();

        await fixture.Handle();

        fixture.DistributedCache.Verify(c => c.SetAsync(
            fixture.MessageId,
            It.Is<byte[]>(v => System.Text.Encoding.UTF8.GetString(v) == "OK"),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task Handle_WhenHandlingCommand_ThenShouldLogInformation()
    {
        var fixture = new RunHealthCheckCommandHandlerTestsFixture();

        await fixture.Handle();

        fixture.Logger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
    }
}

public class RunHealthCheckCommandHandlerTestsFixture
{
    public Mock<IDistributedCache> DistributedCache { get; }
    public Mock<ILogger<RunHealthCheckCommandHandler>> Logger { get; }
    public string MessageId { get; }
    public Mock<IMessageHandlerContext> MessageHandlerContext { get; }
    public IHandleMessages<RunHealthCheckCommand> CommandHandler { get; }

    public RunHealthCheckCommandHandlerTestsFixture()
    {
        DistributedCache = new Mock<IDistributedCache>();
        Logger = new Mock<ILogger<RunHealthCheckCommandHandler>>();
        MessageId = Guid.NewGuid().ToString();
        MessageHandlerContext = new Mock<IMessageHandlerContext>();
        MessageHandlerContext.SetupGet(c => c.MessageId).Returns(MessageId);
        CommandHandler = new RunHealthCheckCommandHandler(DistributedCache.Object, Logger.Object);
    }

    public Task Handle()
    {
        return CommandHandler.Handle(new RunHealthCheckCommand(), MessageHandlerContext.Object);
    }
}
