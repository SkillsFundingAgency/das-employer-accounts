namespace SFA.DAS.EmployerAccounts.Commands.SupportResendInvitationCommand;

public partial class SupportResendInvitationCommandValidator : IValidator<SupportResendInvitationCommand>
{
    public ValidationResult Validate(SupportResendInvitationCommand item)
    {
        var validationResult = new ValidationResult();

        if (string.IsNullOrWhiteSpace(item.Email))
            validationResult.AddError("Email", "No Email supplied");

        if (string.IsNullOrEmpty(item.HashedAccountId))
            validationResult.AddError("HashedId", "No HashedId supplied");

        return validationResult;
    }

    public Task<ValidationResult> ValidateAsync(SupportResendInvitationCommand item)
    {
        return Task.FromResult(Validate(item));
    }
}