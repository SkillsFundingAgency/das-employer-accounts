using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.SendNotification;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.MessageHandlers.EventHandlers.EmployerAccounts;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.EmployerAccounts.Models.UserProfile;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.MessageHandlers.UnitTests.EventHandlers.EmployerAccounts
{
    [TestFixture]
    [Parallelizable]
    public class CreatedAccountTaskListCompleteEventNotificationHandlerTests
    {
        [Test, MoqAutoData]
        public async Task Handle_WhenHandlingEvent_ThenShouldSendNotificationToCorrectUser(
            User user,
            CreatedAccountTaskListCompleteEvent createdAccountEvent,
            [Frozen] Mock<IUserAccountRepository> userRepositoryMock,
            [Frozen] Mock<IMessageHandlerContext> messageHandlerContext,
            [Frozen] Mock<IMediator> mediatorMock,
            CreatedAccountTaskListCompleteEventNotificationHandler eventHandler
            )
        {
            // Arrange 
            userRepositoryMock.Setup(m => m.GetUserByRef(createdAccountEvent.UserRef)).ReturnsAsync(user);

            SendNotificationCommand resultCommand = new SendNotificationCommand();
            mediatorMock.Setup(m => m.Send(It.IsAny<SendNotificationCommand>(), It.IsAny<CancellationToken>()))
                .Callback((IRequest request, CancellationToken cancelToken) =>
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
            CreatedAccountTaskListCompleteEvent createdAccountEvent,
            [Frozen] Mock<IUserAccountRepository> userRepositoryMock,
            [Frozen] Mock<IMessageHandlerContext> messageHandlerContext,
            [Frozen] Mock<IMediator> mediatorMock,
            [Frozen] EmployerAccountsConfiguration configuration,
            CreatedAccountTaskListCompleteEventNotificationHandler eventHandler
            )
        {
            // Arrange
            const string accountBase = "http://accounts.test.url";
            const string notificationPath = "/settings/notifications";
            userRepositoryMock.Setup(m => m.GetUserByRef(createdAccountEvent.UserRef)).ReturnsAsync(user);
            configuration.EmployerAccountsBaseUrl = accountBase;

            var resultCommand = new SendNotificationCommand();
            mediatorMock.Setup(m => m.Send(It.IsAny<SendNotificationCommand>(), It.IsAny<CancellationToken>()))
                .Callback((IRequest request, CancellationToken cancelToken) =>
                {
                    resultCommand = request as SendNotificationCommand;
                });


            // Act
            await eventHandler.Handle(createdAccountEvent, messageHandlerContext.Object);

            // Assert
            resultCommand.Email.Tokens.Should().Contain(new KeyValuePair<string, string>("user_first_name", user.FirstName));
            resultCommand.Email.Tokens.Should().Contain(new KeyValuePair<string, string>("employer_name", createdAccountEvent.Name));
            resultCommand.Email.Tokens.Should().Contain(new KeyValuePair<string, string>("unsubscribe_url", accountBase + notificationPath));
        }
    }
}
