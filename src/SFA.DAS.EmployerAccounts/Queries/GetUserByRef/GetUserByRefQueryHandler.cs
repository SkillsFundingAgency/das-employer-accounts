using System.Threading;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmployerAccounts.Queries.GetUserByRef;

public class GetUserByRefQueryHandler(
    IUserRepository repository,
    IValidator<GetUserByRefQuery> validator,
    ILogger<GetUserByRefQueryHandler> logger)
    : IRequestHandler<GetUserByRefQuery, GetUserByRefResponse>
{
    public async Task<GetUserByRefResponse> Handle(GetUserByRefQuery message, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(message);

        if (!validationResult.IsValid())
        {
            throw new InvalidRequestException(validationResult.ValidationDictionary);
        }

        logger.LogDebug("Getting user with ref {UserRef}", message.UserRef);

        var user = await repository.GetUserByRef(message.UserRef);

        if (user == null)
        {
            validationResult.AddError(nameof(message.UserRef), "User does not exist");
            throw new InvalidRequestException(validationResult.ValidationDictionary);
        }

        return new GetUserByRefResponse { User = user };
    }
}