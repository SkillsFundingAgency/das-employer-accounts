using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.LevyDormancy;
using SFA.DAS.EmployerAccounts.MessageHandlers.EventHandlers.EmployerFinance;
using SFA.DAS.EmployerFinance.Messages.Events;

namespace SFA.DAS.EmployerAccounts.MessageHandlers.UnitTests.EventHandlers.EmployerFinance;

[TestFixture]
public class RefreshEmployerLevyDataAccountLevyStatusProjectionHandlerTests
{
    [Test]
    public async Task WhenAccountLevelEventIsHandled_UpsertCommandIsSent()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var handler = new RefreshEmployerLevyDataAccountLevyStatusProjectionHandler(mediator.Object);
        var refreshedAt = new DateTime(2026, 6, 1);
        var lastDeclaration = new DateTime(2024, 3, 15);

        // Act
        await handler.Handle(new RefreshEmployerLevyDataCompletedEvent
        {
            AccountId = 1,
            LastLevyDeclarationDate = lastDeclaration,
            Created = refreshedAt
        }, null);

        // Assert
        mediator.Verify(x => x.Send(It.Is<UpsertEmployerAccountLevyStatusCommand>(c =>
            c.AccountId == 1 &&
            c.LastLevyDeclarationDate == lastDeclaration &&
            c.RefreshedAt == refreshedAt), CancellationToken.None), Times.Once);
    }
}
