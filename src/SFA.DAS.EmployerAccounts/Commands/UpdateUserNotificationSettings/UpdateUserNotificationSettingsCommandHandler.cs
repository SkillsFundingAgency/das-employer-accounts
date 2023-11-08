using System.Threading;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerAccounts.Audit.Types;
using SFA.DAS.EmployerAccounts.Commands.AuditCommand;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models;

namespace SFA.DAS.EmployerAccounts.Commands.UpdateUserNotificationSettings;

public class UpdateUserNotificationSettingsCommandHandler : IRequestHandler<UpdateUserNotificationSettingsCommand>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IValidator<UpdateUserNotificationSettingsCommand> _validator;
    private readonly IMediator _mediator;
    private readonly ILogger<UpdateUserNotificationSettingsCommandHandler> _logger;

    public UpdateUserNotificationSettingsCommandHandler(IAccountRepository accountRepository,
        IValidator<UpdateUserNotificationSettingsCommand> validator,
        IMediator mediator, 
        ILogger<UpdateUserNotificationSettingsCommandHandler> logger)
    {
        _accountRepository = accountRepository;
        _validator = validator;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(UpdateUserNotificationSettingsCommand message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("{TypeName} processing starting.", nameof(UpdateUserNotificationSettingsCommandHandler));
        var validationResult = _validator.Validate(message);

        if (!validationResult.IsValid())
        {
            throw new InvalidRequestException(validationResult.ValidationDictionary);
        }

        // var tasks = message.Settings.Select(setting => _mediator.Send(CreateAuditCommand(setting), cancellationToken)).ToList();
        //
        // tasks.Add(_accountRepository.UpdateUserAccountSettings(message.UserRef, message.Settings));
        //
        // await Task.WhenAll(tasks);

        _logger.LogInformation("{TypeName} Updating User Account Settings.", nameof(UpdateUserNotificationSettingsCommandHandler));
        await _accountRepository.UpdateUserAccountSettings(message.UserRef, message.Settings);

        foreach (var setting in message.Settings)
        {
            _logger.LogInformation("{TypeName} Creating audit entry for setting '{Setting}'.", nameof(UpdateUserNotificationSettingsCommandHandler), setting.Name);
            var createAuditCommand = CreateAuditCommand(setting);
            await _mediator.Send(createAuditCommand, cancellationToken);
        }

        // var tasks = message.Settings.Select(setting => _mediator.Send(CreateAuditCommand(setting), cancellationToken)).ToList();
        //
        // tasks.Add(_accountRepository.UpdateUserAccountSettings(message.UserRef, message.Settings));
        //
        // await Task.WhenAll(tasks);
        
        _logger.LogInformation("{TypeName} processing completed.", nameof(UpdateUserNotificationSettingsCommandHandler));
    }

    private static CreateAuditCommand CreateAuditCommand(UserNotificationSetting setting)
    {
        return new CreateAuditCommand
        {
            EasAuditMessage = new AuditMessage
            {
                Category = "UPDATED",
                Description = $"User {setting.UserId} has updated email notification setting for account {setting.HashedAccountId}",
                ChangedProperties = new List<PropertyUpdate>
                {
                    new()
                    {
                        PropertyName = "ReceiveNotifications",
                        NewValue = setting.ReceiveNotifications.ToString()
                    }
                },
                RelatedEntities = new List<AuditEntity>
                {
                    new() { Id = setting.UserId.ToString(), Type = "User" }
                },
                AffectedEntity = new AuditEntity { Type = "UserAccountSetting", Id = setting.Id.ToString() }
            }
        };
    }
}