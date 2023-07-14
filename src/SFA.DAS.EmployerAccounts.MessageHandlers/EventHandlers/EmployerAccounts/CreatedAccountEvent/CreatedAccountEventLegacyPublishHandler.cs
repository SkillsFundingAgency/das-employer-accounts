using SFA.DAS.EmployerAccounts.Commands.SendNotification;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Events.Messages;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.Notifications.Api.Types;

namespace SFA.DAS.EmployerAccounts.MessageHandlers.EventHandlers.EmployerAccounts;

public class CreatedAccountEventLegacyPublishHandler : IHandleMessages<CreatedAccountEvent>
{
    private readonly ILegacyTopicMessagePublisher _messagePublisher;
    private readonly ILogger<CreatedAccountEventLegacyPublishHandler> _logger;
    private readonly IUserAccountRepository _userRepository;
    private readonly IMediator _mediator;
    private const string EmployerAccountCreatedTemplateId = "EmployerAccountCreated";

    public CreatedAccountEventLegacyPublishHandler(
        ILegacyTopicMessagePublisher messagePublisher,
        ILogger<CreatedAccountEventLegacyPublishHandler> logger,
        IUserAccountRepository userRepository,
        IMediator mediator)
    {
        _messagePublisher = messagePublisher;
        _logger = logger;
        _userRepository = userRepository;
        _mediator = mediator;
    }

    public async Task Handle(CreatedAccountEvent message, IMessageHandlerContext context)
    {
        _logger.LogInformation($"Starting {nameof(CreatedAccountEventLegacyPublishHandler)} handler for accountId: '{message.AccountId}'.");

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

        await _messagePublisher.PublishAsync(
           new AccountCreatedMessage(
               message.AccountId,
               message.UserName,
               message.UserRef.ToString()));

        _logger.LogInformation($"Completed {nameof(CreatedAccountEventLegacyPublishHandler)} handler.");
    }
}