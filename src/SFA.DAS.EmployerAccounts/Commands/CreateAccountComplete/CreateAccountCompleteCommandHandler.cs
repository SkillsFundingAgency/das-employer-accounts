using System.Threading;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerAccounts.Queries.GetUserByRef;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerAccounts.Commands.CreateAccountComplete;

public class CreateAccountCompleteCommandHandler : IRequestHandler<CreateAccountCompleteCommand>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly IMediator _mediator;
    private readonly IValidator<CreateAccountCompleteCommand> _validator;
    private readonly ILogger<CreateAccountCompleteCommandHandler> _logger;

    public CreateAccountCompleteCommandHandler(IEventPublisher eventPublisher, 
        IMediator mediator,
        IValidator<CreateAccountCompleteCommand> validator,
        ILogger<CreateAccountCompleteCommandHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _mediator = mediator;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Unit> Handle(CreateAccountCompleteCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting processing of {TypeName}. Request: '{Request}'.", nameof(CreateAccountCompleteCommandHandler), request);
        
        ValidateRequest(request);
        
        var userResponse = await _mediator.Send(new GetUserByRefQuery { UserRef = request.ExternalUserId }, cancellationToken);

        var externalUserId = Guid.Parse(request.ExternalUserId);
        
        await PublishAccountCreatedMessage(request.AccountId, request.HashedAccountId, request.PublicHashedAccountId, request.OrganisationName, userResponse.User.FullName, externalUserId);
        
        _logger.LogInformation("Completed processing of {TypeName}.", nameof(CreateAccountCompleteCommandHandler));

        return Unit.Value;
    }
    
    private void ValidateRequest(CreateAccountCompleteCommand message)
    {
        var validationResult = _validator.Validate(message);

        if (!validationResult.IsValid())
            throw new InvalidRequestException(validationResult.ValidationDictionary);
    }

    private Task PublishAccountCreatedMessage(long accountId, string hashedId, string publicHashedId, string name, string createdByName, Guid userRef)
    {
        return _eventPublisher.Publish(new CreatedAccountEvent
        {
            AccountId = accountId,
            HashedId = hashedId,
            PublicHashedId = publicHashedId,
            Name = name,
            UserName = createdByName,
            UserRef = userRef,
            Created = DateTime.UtcNow
        });
    }
}