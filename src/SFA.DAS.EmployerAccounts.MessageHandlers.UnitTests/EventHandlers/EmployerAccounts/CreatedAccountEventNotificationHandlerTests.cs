using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.SendNotification;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.MessageHandlers.EventHandlers.EmployerAccounts;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.EmployerAccounts.Models.UserProfile;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.MessageHandlers.UnitTests.EventHandlers.EmployerAccounts
{
    [TestFixture]
    [Parallelizable]
    public class CreatedAccountEventNotificationHandlerTests
    {
        [Test, MoqAutoData]
        public async Task Handle_WhenHandlingEvent_ThenShouldSendNotificationToCorrectUser(
            User user,
            CreatedAccountEvent createdAccountEvent,
            [Frozen] Mock<IUserAccountRepository> userRepositoryMock,
            [Frozen] Mock<IMessageHandlerContext> messageHandlerContext,
            [Frozen] Mock<IMediator> mediatorMock,
            CreatedAccountEventNotificationHandler eventHandler
            )
        {
            // Arrange 
            userRepositoryMock.Setup(m => m.GetUserByRef(createdAccountEvent.UserRef)).ReturnsAsync(user);

            SendNotificationCommand resultCommand = new SendNotificationCommand();
            mediatorMock.Setup(m => m.Send(It.IsAny<SendNotificationCommand>(), It.IsAny<CancellationToken>()))
                .Callback((IRequest<Unit> request, CancellationToken cancelToken) =>
                {
                    resultCommand = request as SendNotificationCommand;
                });


            // Act
            await eventHandler.Handle(createdAccountEvent, messageHandlerContext.Object);

            // Assert
            resultCommand.Email.RecipientsAddress.Should().Be(user.Email);
        }

        [Test, MoqAutoData]
        public async Task Handle_WhenHandlingEvent_ThenShouldSendNotificationWithCorrectTokens(
            User user,
            CreatedAccountEvent createdAccountEvent,
            [Frozen] Mock<IUserAccountRepository> userRepositoryMock,
            [Frozen] Mock<IMessageHandlerContext> messageHandlerContext,
            [Frozen] Mock<IMediator> mediatorMock,
            CreatedAccountEventNotificationHandler eventHandler
            )
        {
            // Arrange 
            userRepositoryMock.Setup(m => m.GetUserByRef(createdAccountEvent.UserRef)).ReturnsAsync(user);

            SendNotificationCommand resultCommand = new SendNotificationCommand();
            mediatorMock.Setup(m => m.Send(It.IsAny<SendNotificationCommand>(), It.IsAny<CancellationToken>()))
                .Callback((IRequest<Unit> request, CancellationToken cancelToken) =>
                {
                    resultCommand = request as SendNotificationCommand;
                });


            // Act
            await eventHandler.Handle(createdAccountEvent, messageHandlerContext.Object);

            // Assert
            resultCommand.Email.Tokens.Should().Contain(new System.Collections.Generic.KeyValuePair<string, string>( "user_first_name", user.FirstName));
            resultCommand.Email.Tokens.Should().Contain(new System.Collections.Generic.KeyValuePair<string, string>("employer_name", createdAccountEvent.Name));
        }
    }
}
