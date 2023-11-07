using System.Threading;
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

    public UpdateUserNotificationSettingsCommandHandler(IAccountRepository accountRepository,
        IValidator<UpdateUserNotificationSettingsCommand> validator, IMediator mediator)
    {
        _accountRepository = accountRepository;
        _validator = validator;
        _mediator = mediator;
    }

    public async Task Handle(UpdateUserNotificationSettingsCommand message, CancellationToken cancellationToken)
    {
        var validationResult = _validator.Validate(message);

        if (!validationResult.IsValid())
        {
            throw new InvalidRequestException(validationResult.ValidationDictionary);
        }

        var tasks = message.Settings.Select(AddAuditEntry).ToList();

        tasks.Add(_accountRepository.UpdateUserAccountSettings(message.UserRef, message.Settings));

        await Task.WhenAll(tasks);
    }

    private async Task AddAuditEntry(UserNotificationSetting setting)
    {
        await _mediator.Send(new CreateAuditCommand
        {
            EasAuditMessage = new AuditMessage
            {
                Category = "UPDATED",
                Description =
                    $"User {setting.UserId} has updated email notification setting for account {setting.HashedAccountId}",
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
        });
    }
}