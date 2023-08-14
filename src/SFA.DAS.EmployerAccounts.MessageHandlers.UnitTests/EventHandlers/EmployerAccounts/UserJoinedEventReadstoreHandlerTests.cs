using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using MediatR;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.MessageHandlers.EventHandlers.EmployerAccounts;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.EmployerAccounts.ReadStore.Application.Commands;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.MessageHandlers.UnitTests.EventHandlers.EmployerAccounts
{
    [TestFixture]
    [Parallelizable]
    public class UserJoinedEventReadstoreHandlerTests
    {
        [Test, MoqAutoData]
        public async Task Handle_WhenHandlingEvent_ThenShouldSendCreateAccountUserCommand(
            UserJoinedEvent userJoinedEvent,
            [Frozen] Mock<IMessageHandlerContext> messageHandlerContext,
            [Frozen] Mock<IMediator> mediatorMock,
            UserJoinedEventReadstoreHandler eventHandler
            )
        {
            // Arrange 

            // Act
            await eventHandler.Handle(userJoinedEvent, messageHandlerContext.Object);

            // Assert
            mediatorMock.Verify(m => m.Send(It.Is<CreateAccountUserCommand>(x => x.UserRef == userJoinedEvent.UserRef), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
