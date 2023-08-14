using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Events.Messages;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.MessageHandlers.EventHandlers.EmployerAccounts;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.MessageHandlers.UnitTests.EventHandlers.EmployerAccounts
{
    [TestFixture]
    [Parallelizable]
    public class UserJoinedEventLegacyPublishHandlerTests
    {
        [Test, MoqAutoData]
        public async Task Handle_WhenHandlingEvent_ThenShouldSendCreateAccountUserCommand(
            UserJoinedEvent userJoinedEvent,
            [Frozen] Mock<IMessageHandlerContext> messageHandlerContext,
            [Frozen] Mock<ILegacyTopicMessagePublisher> legacyTopicMessagePublisherMock,
            UserJoinedEventLegacyPublishHandler eventHandler
            )
        {
            // Arrange 

            // Act
            await eventHandler.Handle(userJoinedEvent, messageHandlerContext.Object);

            // Assert
            legacyTopicMessagePublisherMock.Verify(m => m.PublishAsync(It.Is<UserJoinedMessage>(x => x.CreatorUserRef == userJoinedEvent.UserRef.ToString())), Times.Once);
        }
    }
}
