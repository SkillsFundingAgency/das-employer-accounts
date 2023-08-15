using System.Threading;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerAccounts.Queries.GetUserByRef;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerAccounts.Commands.CreateAccountComplete;

public class SendAccountTaskListCompleteNotificationCommandHandler : IRequestHandler<SendAccountTaskListCompleteNotificationCommand>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly IMediator _mediator;
    private readonly IValidator<SendAccountTaskListCompleteNotificationCommand> _validator;
    private readonly ILogger<SendAccountTaskListCompleteNotificationCommandHandler> _logger;

    public SendAccountTaskListCompleteNotificationCommandHandler(IEventPublisher eventPublisher, 
        IMediator mediator,
        IValidator<SendAccountTaskListCompleteNotificationCommand> validator,
        ILogger<SendAccountTaskListCompleteNotificationCommandHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _mediator = mediator;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Unit> Handle(SendAccountTaskListCompleteNotificationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting processing of {TypeName}. Request: '{Request}'.", nameof(SendAccountTaskListCompleteNotificationCommandHandler), request);
        
        ValidateRequest(request);
        
        var userResponse = await _mediator.Send(new GetUserByRefQuery { UserRef = request.ExternalUserId }, cancellationToken);

        var externalUserId = Guid.Parse(request.ExternalUserId);
        
        await PublishAccountCreatedMessage(request.AccountId, request.HashedAccountId, request.PublicHashedAccountId, request.OrganisationName, userResponse.User.FullName, externalUserId);
        
        _logger.LogInformation("Completed processing of {TypeName}.", nameof(SendAccountTaskListCompleteNotificationCommandHandler));

        return Unit.Value;
    }
    
    private void ValidateRequest(SendAccountTaskListCompleteNotificationCommand message)
    {
        var validationResult = _validator.Validate(message);

        if (!validationResult.IsValid())
            throw new InvalidRequestException(validationResult.ValidationDictionary);
    }

    private Task PublishAccountCreatedMessage(long accountId, string hashedId, string publicHashedId, string name, string createdByName, Guid userRef)
    {
        return _eventPublisher.Publish(new CreatedAccountTaskListCompleteEvent
        {
            AccountId = accountId,
            HashedId = hashedId,
            PublicHashedId = publicHashedId,
            Name = name,
            UserName = createdByName,
            UserRef = userRef
        });
    }
}