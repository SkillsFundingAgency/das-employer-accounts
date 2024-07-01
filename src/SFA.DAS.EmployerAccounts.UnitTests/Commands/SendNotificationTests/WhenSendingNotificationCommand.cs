using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.SendNotification;
using SFA.DAS.EmployerAccounts.TestCommon;
using SFA.DAS.Notifications.Messages.Commands;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.SendNotificationTests;

public class WhenSendingNotificationCommand
{
    private SendNotificationCommandHandler _sendNotificationCommandHandler;
    private Mock<IValidator<SendNotificationCommand>> _validator;
    private Mock<ILogger<SendNotificationCommandHandler>> _logger;
    private Mock<IMessageSession> _publisher;

    [SetUp]
    public void Arrange()
    {
        _logger = new Mock<ILogger<SendNotificationCommandHandler>>();

        _validator = new Mock<IValidator<SendNotificationCommand>>();
        _validator.Setup(x => x.Validate(It.IsAny<SendNotificationCommand>())).Returns(new ValidationResult());
        _publisher = new Mock<IMessageSession>();
        _sendNotificationCommandHandler =
            new SendNotificationCommandHandler(_validator.Object, _logger.Object, _publisher.Object);
    }

    [Test]
    public async Task ThenTheCommandIsValidated()
    {
        //Act
        await _sendNotificationCommandHandler.Handle(new SendNotificationCommand(), CancellationToken.None);

        //Assert
        _validator.Verify(x => x.Validate(It.IsAny<SendNotificationCommand>()), Times.Once());
    }

    [Test]
    public async Task ThenTheNotificationIsPublished()
    {
        //Arrange
        var sendNotificationCommand = new SendNotificationCommand
        {
            RecipientsAddress = "test@test.com",
            TemplateId = "12345",
            Tokens = new Dictionary<string, string> { { "string", "value" } }
        };

        //Act
        await _sendNotificationCommandHandler.Handle(sendNotificationCommand, CancellationToken.None);

        //Assert
        _publisher.Verify(x => x.Send(It.Is<SendEmailCommand>(
            c => c.RecipientsAddress.Equals(sendNotificationCommand.RecipientsAddress)
                 && c.TemplateId.Equals(sendNotificationCommand.TemplateId)
                 && c.Tokens.Equals(sendNotificationCommand.Tokens)
        ), It.IsAny<SendOptions>()));
    }

    [Test]
    public void ThenAnInvalidRequestExceptionIsThrownAndAnInfoLevelMessageIsLoggedIfTheCommandIsNotValid()
    {
        //Arrange
        _validator.Setup(x => x.Validate(It.IsAny<SendNotificationCommand>())).Returns(new ValidationResult
            { ValidationDictionary = new Dictionary<string, string> { { "", "" } } });

        //Act
        Assert.ThrowsAsync<InvalidRequestException>(async () =>
            await _sendNotificationCommandHandler.Handle(new SendNotificationCommand(), CancellationToken.None));

        //Assert
        _logger.VerifyLogging("SendNotificationCommandHandler Invalid Request", LogLevel.Information, Times.Once());
    }
}