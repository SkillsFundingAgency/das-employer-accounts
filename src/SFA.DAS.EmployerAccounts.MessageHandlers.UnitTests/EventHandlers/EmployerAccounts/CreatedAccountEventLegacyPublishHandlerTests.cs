// using AutoFixture.NUnit3;
// using Moq;
// using NServiceBus;
// using System.Threading.Tasks;
// using NUnit.Framework;
// using SFA.DAS.EmployerAccounts.Interfaces;
// using SFA.DAS.EmployerAccounts.MessageHandlers.EventHandlers.EmployerAccounts;
// using SFA.DAS.EmployerAccounts.Messages.Events;
// using SFA.DAS.Testing.AutoFixture;
// using SFA.DAS.EmployerAccounts.Events.Messages;
//
// namespace SFA.DAS.EmployerAccounts.MessageHandlers.UnitTests.EventHandlers.EmployerAccounts
// {
//     [TestFixture]
//     [Parallelizable]
//     public class CreatedAccountEventLegacyPublishHandlerTests
//     {
//         [Test, MoqAutoData]
//         public async Task Handle_WhenHandlingEvent_ThenShouldSendAccountCreatedMessage(
//             CreatedAccountEvent createdAccountEvent,
//             [Frozen] Mock<IMessageHandlerContext> messageHandlerContext,
//             [Frozen] Mock<ILegacyTopicMessagePublisher> legacyTopicMessagePublisherMock,
//             CreatedAccountEventLegacyPublishHandler eventHandler
//             )
//         {
//             // Arrange 
//
//             // Act
//             await eventHandler.Handle(createdAccountEvent, messageHandlerContext.Object);
//
//             // Assert
//             legacyTopicMessagePublisherMock
//                 .Verify(m => m.PublishAsync(
//                     It.Is<AccountCreatedMessage>(x => x.AccountId == createdAccountEvent.AccountId && x.CreatorUserRef == createdAccountEvent.UserRef.ToString())), 
//                     Times.Once);
//         }
//     }
// }
