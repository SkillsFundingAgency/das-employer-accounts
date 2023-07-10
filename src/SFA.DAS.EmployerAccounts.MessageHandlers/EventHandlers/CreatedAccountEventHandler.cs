using SFA.DAS.EmployerAccounts.Commands.SendNotification;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Events.Messages;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.Notifications.Api.Types;

namespace SFA.DAS.EmployerAccounts.MessageHandlers.EventHandlers;

public class CreatedAccountEventHandler : IHandleMessages<CreatedAccountEvent>
{
    private readonly ILegacyTopicMessagePublisher _messagePublisher;
    private readonly ILogger<CreatedAccountEventHandler> _logger;
    private readonly IUserAccountRepository _userRepository;
    private readonly IMediator _mediator;
    private readonly EmployerAccountsConfiguration _employerAccountsConfiguration;

    public CreatedAccountEventHandler(
        ILegacyTopicMessagePublisher messagePublisher, 
        ILogger<CreatedAccountEventHandler> logger,
        IUserAccountRepository userRepository,
        IMediator mediator,
        EmployerAccountsConfiguration employerAccountsConfiguration)
    {
        _messagePublisher = messagePublisher;
        _logger = logger;
        _userRepository = userRepository;
        _mediator = mediator;
        _employerAccountsConfiguration = employerAccountsConfiguration;
    }

    public async Task Handle(CreatedAccountEvent message, IMessageHandlerContext context)
    {
        _logger.LogInformation($"Starting {nameof(CreatedAccountEventHandler)} handler for accountId: '{message.AccountId}'.");

        var existingUser = await _userRepository.GetUserByRef(message.UserRef);

        var templateId = "";

        await _mediator.Send(new SendNotificationCommand
        {
            Email = new Email
            {
                RecipientsAddress = existingUser.Email,
                TemplateId = templateId,
                ReplyToAddress = "noreply@sfa.gov.uk",
                Subject = "x",
                SystemId = "x",
                Tokens = new Dictionary<string, string> {
                    { "account_name", message.Name },
                    { "first_name", existingUser.FirstName },
                    { "base_url", _employerAccountsConfiguration.EmployerAccountsBaseUrl }
                }
            }
        });

        await _messagePublisher.PublishAsync(
           new AccountCreatedMessage(
               message.AccountId,
               message.UserName,
               message.UserRef.ToString()));

        _logger.LogInformation($"Completed {nameof(CreatedAccountEventHandler)} handler.");
    }
}