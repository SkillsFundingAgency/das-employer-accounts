﻿using SFA.DAS.EmployerAccounts.Commands.SendNotification;
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
    private const string EmployerAccountCreatedTemplateId = "EmployerAccountCreated";

    public CreatedAccountTaskListCompleteEventNotificationHandler(
        ILogger<CreatedAccountTaskListCompleteEventNotificationHandler> logger,
        IUserAccountRepository userRepository,
        IMediator mediator,
        EmployerAccountsConfiguration configuration)
    {
        _logger = logger;
        _userRepository = userRepository;
        _mediator = mediator;
        _configuration = configuration;
    }

    public async Task Handle(CreatedAccountTaskListCompleteEvent message, IMessageHandlerContext context)
    {
        _logger.LogInformation(
            $"Starting {nameof(CreatedAccountTaskListCompleteEventNotificationHandler)} handler for accountId: '{message.AccountId}'.");

        var existingUser = await _userRepository.GetUserByRef(message.UserRef);

        await _mediator.Send(new SendNotificationCommand
        {
            RecipientsAddress = existingUser.Email,
            TemplateId = EmployerAccountCreatedTemplateId,
            Tokens = new Dictionary<string, string>
            {
                { "user_first_name", existingUser.FirstName },
                { "user_name", existingUser.FullName },
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