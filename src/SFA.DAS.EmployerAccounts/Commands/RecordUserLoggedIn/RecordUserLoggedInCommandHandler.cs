using System.Threading;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerAccounts.Data.Contracts;

namespace SFA.DAS.EmployerAccounts.Commands.RecordUserLoggedIn;

public class RecordUserLoggedInCommandHandler : IRequestHandler<RecordUserLoggedInCommand>
{
    private readonly IUserAccountRepository _userRepository;
    private readonly ILogger<RecordUserLoggedInCommandHandler> _logger;

    public RecordUserLoggedInCommandHandler(
        IUserAccountRepository userRepository,
        ILogger<RecordUserLoggedInCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task Handle(RecordUserLoggedInCommand message, CancellationToken cancellationToken)
    {
        if (Guid.TryParse(message.UserRef, out Guid userId))
        {
            await _userRepository.RecordLogin(userId);
        }
        else
        {
            _logger.LogInformation("Unable to record user logging in as the UserRef {0} is not a GUID", message.UserRef);
        }
    }
}