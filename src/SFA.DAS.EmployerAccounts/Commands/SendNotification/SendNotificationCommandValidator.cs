namespace SFA.DAS.EmployerAccounts.Commands.SendNotification;

public class SendNotificationCommandValidator : IValidator<SendNotificationCommand>
{
    public ValidationResult Validate(SendNotificationCommand item)
    {
        var validationResult = new ValidationResult();

        if (string.IsNullOrEmpty(item.RecipientsAddress))
        {
            validationResult.AddError(nameof(item.RecipientsAddress), "RecipientsAddress has not been supplied");
        }
            
        if (string.IsNullOrEmpty(item.TemplateId))
        {
            validationResult.AddError(nameof(item.TemplateId), "TemplateId has not been supplied");
        }

        return validationResult;
    }

    public Task<ValidationResult> ValidateAsync(SendNotificationCommand item)
    {
        return Task.FromResult(Validate(item));
    }
}