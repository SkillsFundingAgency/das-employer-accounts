using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.CreateAccountComplete;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.EmployerAccounts.Models.UserProfile;
using SFA.DAS.EmployerAccounts.Queries.GetUserByRef;
using SFA.DAS.NServiceBus.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.CreateAccountComplete;

public class WhenICallSendAccountTaskListCompleteNotification
{
    [Test, MoqAutoData]
    public async Task ThenACreatedAccountEventIsPublished(
        User user,
        SendAccountTaskListCompleteNotificationCommand command,
        [Frozen] Mock<IEventPublisher> eventPublisher,
        [Frozen] Mock<IMediator> mediatorMock,
        [Frozen] Mock<IValidator<SendAccountTaskListCompleteNotificationCommand>> validatorMock,
        SendAccountTaskListCompleteNotificationCommandHandler handler
        )
    {
        //Arrange
        command.ExternalUserId = user.Ref.ToString();

        validatorMock.Setup(x => x.Validate(It.IsAny<SendAccountTaskListCompleteNotificationCommand>()))
            .Returns(new ValidationResult { ValidationDictionary = new Dictionary<string, string>() });

        mediatorMock.Setup(x => x.Send(It.IsAny<GetUserByRefQuery>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(new GetUserByRefResponse { User = user });

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
        createdAccountEvent.HashedId.Should().Be(command.HashedAccountId);
        createdAccountEvent.PublicHashedId.Should().Be(command.PublicHashedAccountId);
        createdAccountEvent.Name.Should().Be(command.OrganisationName);
        createdAccountEvent.UserName.Should().Be(user.FullName);
        createdAccountEvent.UserRef.Should().Be(user.Ref);
    }
}