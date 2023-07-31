// using SFA.DAS.EmployerAccounts.Events.Messages;
// using SFA.DAS.EmployerAccounts.Interfaces;
// using SFA.DAS.EmployerAccounts.Messages.Events;
//
// namespace SFA.DAS.EmployerAccounts.MessageHandlers.EventHandlers.EmployerAccounts;
//
// public class CreatedAccountEventLegacyPublishHandler : IHandleMessages<CreatedAccountEvent>
// {
//     private readonly ILegacyTopicMessagePublisher _messagePublisher;
//     private readonly ILogger<CreatedAccountEventLegacyPublishHandler> _logger;
//
//     public CreatedAccountEventLegacyPublishHandler(
//         ILegacyTopicMessagePublisher messagePublisher,
//         ILogger<CreatedAccountEventLegacyPublishHandler> logger)
//     {
//         _messagePublisher = messagePublisher;
//         _logger = logger;
//     }
//
//     public async Task Handle(CreatedAccountEvent message, IMessageHandlerContext context)
//     {
//         _logger.LogInformation($"Starting {nameof(CreatedAccountEventLegacyPublishHandler)} handler for accountId: '{message.AccountId}'.");
//
//         await _messagePublisher.PublishAsync(
//            new AccountCreatedMessage(
//                message.AccountId,
//                message.UserName,
//                message.UserRef.ToString()));
//
//         _logger.LogInformation($"Completed {nameof(CreatedAccountEventLegacyPublishHandler)} handler.");
//     }
// }