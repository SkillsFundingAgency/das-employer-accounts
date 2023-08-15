using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.CreateAccountComplete;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.NServiceBus.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.CreateAccountComplete;

public class WhenICallSendAccountTaskListCompleteNotification
{
    [Test, MoqAutoData]
    public async Task ThenACreatedAccountEventIsPublished(
        SendAccountTaskListCompleteNotificationCommand command,
        [Frozen] Mock<IEventPublisher> eventPublisher,
        [Frozen] Mock<IValidator<SendAccountTaskListCompleteNotificationCommand>> validatorMock,
        SendAccountTaskListCompleteNotificationCommandHandler handler
        )
    {
        //Arrange
        command.ExternalUserId = Guid.NewGuid().ToString();

        validatorMock.Setup(x => x.Validate(It.IsAny<SendAccountTaskListCompleteNotificationCommand>()))
            .Returns(new ValidationResult { ValidationDictionary = new Dictionary<string, string>() });

        var events = new List<CreatedAccountTaskListCompleteEvent>();
        eventPublisher
            .Setup(x => x.Publish(It.IsAny<CreatedAccountTaskListCompleteEvent>()))
            .Callback((CreatedAccountTaskListCompleteEvent ev) =>
            {
                events.Add(ev);
            });

        //Act
        await handler.Handle(command, CancellationToken.None);

        //Assert
        var createdAccountEvent = events.Single();

        createdAccountEvent.AccountId.Should().Be(command.AccountId);
        createdAccountEvent.Name.Should().Be(command.OrganisationName);
    }
}