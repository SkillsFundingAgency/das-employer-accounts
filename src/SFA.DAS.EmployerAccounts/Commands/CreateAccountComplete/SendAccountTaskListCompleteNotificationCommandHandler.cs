using System.Threading;
using Microsoft.Extensions.Logging;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerAccounts.Commands.CreateAccountComplete;

public class SendAccountTaskListCompleteNotificationCommandHandler : IRequestHandler<SendAccountTaskListCompleteNotificationCommand>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly IValidator<SendAccountTaskListCompleteNotificationCommand> _validator;
    private readonly ILogger<SendAccountTaskListCompleteNotificationCommandHandler> _logger;

    public SendAccountTaskListCompleteNotificationCommandHandler(IEventPublisher eventPublisher, 
        IValidator<SendAccountTaskListCompleteNotificationCommand> validator,
        ILogger<SendAccountTaskListCompleteNotificationCommandHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _validator = validator;
        _logger = logger;
    }

    public async Task Handle(SendAccountTaskListCompleteNotificationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting processing of {TypeName}. Request: '{Request}'.", nameof(SendAccountTaskListCompleteNotificationCommandHandler), request);
        
        ValidateRequest(request);

        var externalUserId = Guid.Parse(request.ExternalUserId);

        await _eventPublisher.Publish(new CreatedAccountTaskListCompleteEvent
        {
            AccountId = request.AccountId,
            Name = request.OrganisationName,
            UserRef = externalUserId
        });
        
        _logger.LogInformation("Completed processing of {TypeName}.", nameof(SendAccountTaskListCompleteNotificationCommandHandler));
    }
    
    private void ValidateRequest(SendAccountTaskListCompleteNotificationCommand message)
    {
        var validationResult = _validator.Validate(message);

        if (!validationResult.IsValid())
            throw new InvalidRequestException(validationResult.ValidationDictionary);
    }
}