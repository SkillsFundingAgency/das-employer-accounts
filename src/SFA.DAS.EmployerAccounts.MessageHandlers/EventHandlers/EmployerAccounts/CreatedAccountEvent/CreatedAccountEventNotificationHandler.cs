using SFA.DAS.EmployerAccounts.Commands.SendNotification;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.Notifications.Api.Types;

namespace SFA.DAS.EmployerAccounts.MessageHandlers.EventHandlers.EmployerAccounts;

public class CreatedAccountEventNotificationHandler : IHandleMessages<CreatedAccountEvent>
{
    private readonly ILogger<CreatedAccountEventNotificationHandler> _logger;
    private readonly IUserAccountRepository _userRepository;
    private readonly IMediator _mediator;
    private readonly EmployerAccountsConfiguration _configuration;
    private const string EmployerAccountCreatedTemplateId = "3d455ac3-876d-4bf8-8a77-07b67c06831d";

    public CreatedAccountEventNotificationHandler(
        ILogger<CreatedAccountEventNotificationHandler> logger,
        IUserAccountRepository userRepository,
        IMediator mediator,
        EmployerAccountsConfiguration configuration)
    {
        _logger = logger;
        _userRepository = userRepository;
        _mediator = mediator;
        _configuration = configuration;
    }

    public async Task Handle(CreatedAccountEvent message, IMessageHandlerContext context)
    {
        _logger.LogInformation(
            $"Starting {nameof(CreatedAccountEventNotificationHandler)} handler for accountId: '{message.AccountId}'.");

        var existingUser = await _userRepository.GetUserByRef(message.UserRef);

        await _mediator.Send(new SendNotificationCommand
        {
            Email = new Email
            {
                RecipientsAddress = existingUser.Email,
                TemplateId = EmployerAccountCreatedTemplateId,
                Tokens = new Dictionary<string, string>
                {
                    { "user_first_name", existingUser.FirstName },
                    { "employer_name", message.Name },
                    {
                        "unsubscribe_url",
                        $"{_configuration.EmployerAccountsBaseUrl}/settings/notifications"
                    }
                }
            }
        });

        _logger.LogInformation($"Completed {nameof(CreatedAccountEventNotificationHandler)} handler.");
    }
}