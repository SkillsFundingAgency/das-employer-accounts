using SFA.DAS.EmployerAccounts.Commands.SendNotification;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.Notifications.Api.Types;

namespace SFA.DAS.EmployerAccounts.MessageHandlers.EventHandlers.EmployerAccounts;

public class CreatedAccountEventNotificationHandler : IHandleMessages<CreatedAccountEvent>
{
    private readonly ILogger<CreatedAccountEventNotificationHandler> _logger;
    private readonly IUserAccountRepository _userRepository;
    private readonly IMediator _mediator;
    private const string EmployerAccountCreatedTemplateId = "EmployerAccountCreated";

    public CreatedAccountEventNotificationHandler(
        ILogger<CreatedAccountEventNotificationHandler> logger,
        IUserAccountRepository userRepository,
        IMediator mediator)
    {
        _logger = logger;
        _userRepository = userRepository;
        _mediator = mediator;
    }

    public async Task Handle(CreatedAccountEvent message, IMessageHandlerContext context)
    {
        _logger.LogInformation($"Starting {nameof(CreatedAccountEventNotificationHandler)} handler for accountId: '{message.AccountId}'.");

        var existingUser = await _userRepository.GetUserByRef(message.UserRef);

        await _mediator.Send(new SendNotificationCommand
        {
            Email = new Email
            {
                RecipientsAddress = existingUser.Email,
                TemplateId = EmployerAccountCreatedTemplateId,
                Tokens = new Dictionary<string, string> {
                    { "user_first_name", existingUser.FirstName },
                    { "employer_name", message.Name }
                }
            }
        });

        _logger.LogInformation($"Completed {nameof(CreatedAccountEventNotificationHandler)} handler.");
    }
}