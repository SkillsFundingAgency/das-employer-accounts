using System.Threading;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Queries.GetUserByRef;
using SFA.DAS.Encoding;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerAccounts.Commands.CreateAccountComplete;

public class CreateAccountCompleteCommandHandler : IRequestHandler<CreateAccountCompleteCommand>
{
    private readonly IEventPublisher _eventPublisher;
    private readonly IMediator _mediator;
    private readonly IEncodingService _encodingService;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IValidator<CreateAccountCompleteCommand> _validator;

    public CreateAccountCompleteCommandHandler(IEventPublisher eventPublisher, 
        IMediator mediator,
        IMembershipRepository membershipRepository,
        IEncodingService encodingService,
        IValidator<CreateAccountCompleteCommand> validator)
    {
        _eventPublisher = eventPublisher;
        _mediator = mediator;
        _membershipRepository = membershipRepository;
        _encodingService = encodingService;
        _validator = validator;
    }

    public async Task<Unit> Handle(CreateAccountCompleteCommand request, CancellationToken cancellationToken)
    {
        ValidateRequest(request);
        
        var externalUserId = Guid.Parse(request.ExternalUserId);
        var userResponse = await _mediator.Send(new GetUserByRefQuery { UserRef = request.HashedAccountId }, cancellationToken);
        var publicHashedAccountId = _encodingService.Encode(userResponse.User.Id, EncodingType.PublicAccountId);

        var caller = await _membershipRepository.GetCaller(userResponse.User.Id, request.ExternalUserId);

        var createdByName = caller.FullName();

        await PublishAccountCreatedMessage(userResponse.User.Id, request.HashedAccountId, publicHashedAccountId, request.OrganisationName, createdByName, externalUserId);

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