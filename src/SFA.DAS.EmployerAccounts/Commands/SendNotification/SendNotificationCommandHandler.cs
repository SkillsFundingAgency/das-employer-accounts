using System.Threading;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Notifications.Messages.Commands;

namespace SFA.DAS.EmployerAccounts.Commands.SendNotification;

public class SendNotificationCommandHandler : IRequestHandler<SendNotificationCommand>
{
    private readonly IValidator<SendNotificationCommand> _validator;
    private readonly ILogger<SendNotificationCommandHandler> _logger;
    private readonly IMessageSession _publisher;

    public SendNotificationCommandHandler(
        IValidator<SendNotificationCommand> validator,
        ILogger<SendNotificationCommandHandler> logger,
        IMessageSession publisher)
    {
        _validator = validator;
        _logger = logger;
        _publisher = publisher;
    }

    public async Task Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        var validationResult = _validator.Validate(request);

        if (!validationResult.IsValid())
        {
            _logger.LogInformation("SendNotificationCommandHandler Invalid Request");
            throw new InvalidRequestException(validationResult.ValidationDictionary);
        }

        try
        {
            await _publisher.Send(new SendEmailCommand(
                request.TemplateId,
                request.RecipientsAddress,
                request.Tokens)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to notifications api");
        }
    }
}