using SFA.DAS.EmployerAccounts.Commands.SendNotification;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Messages.Events;

namespace SFA.DAS.EmployerAccounts.MessageHandlers.EventHandlers.EmployerAccounts;

public class
    CreatedAccountTaskListCompleteEventNotificationHandler : IHandleMessages<CreatedAccountTaskListCompleteEvent>
{
    private readonly ILogger<CreatedAccountTaskListCompleteEventNotificationHandler> _logger;
    private readonly IUserAccountRepository _userRepository;
    private readonly IMediator _mediator;
    private readonly EmployerAccountsConfiguration _configuration;
    private readonly string _employerAccountCreatedTemplateId;

    public CreatedAccountTaskListCompleteEventNotificationHandler(
        ILogger<CreatedAccountTaskListCompleteEventNotificationHandler> logger,
        IUserAccountRepository userRepository,
        IMediator mediator,
        EmployerAccountsConfiguration configuration,
        IConfiguration config)
    {
        _logger = logger;
        _userRepository = userRepository;
        _mediator = mediator;
        _configuration = configuration;
        _employerAccountCreatedTemplateId = config["ResourceEnvironmentName"].Equals("prd", StringComparison.CurrentCultureIgnoreCase) ? "EmployerAccountCreated" : "EmployerAccountCreated_dev";
    }

    public async Task Handle(CreatedAccountTaskListCompleteEvent message, IMessageHandlerContext context)
    {
        _logger.LogInformation(
            $"Starting {nameof(CreatedAccountTaskListCompleteEventNotificationHandler)} handler for accountId: '{message.AccountId}'.");

        var existingUser = await _userRepository.GetUserByRef(message.UserRef);

        _logger.LogInformation($"Sending Email for Template: '{_employerAccountCreatedTemplateId}'.");

        await _mediator.Send(new SendNotificationCommand
        {
            RecipientsAddress = existingUser.Email,
            TemplateId = _employerAccountCreatedTemplateId,
            Tokens = new Dictionary<string, string>
            {
                { "user_first_name", existingUser.FirstName },
                { "employer_name", message.Name },
                {
                    "unsubscribe_url",
                    $"{_configuration.EmployerAccountsBaseUrl}/settings/notifications"
                }
            }
        });

        _logger.LogInformation($"Completed {nameof(CreatedAccountTaskListCompleteEventNotificationHandler)} handler.");
    }
}