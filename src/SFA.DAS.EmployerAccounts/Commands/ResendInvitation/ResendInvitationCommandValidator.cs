namespace SFA.DAS.EmployerAccounts.Commands.ResendInvitation;

public class ResendInvitationCommandValidator : IValidator<ResendInvitationCommand>
{
    public ValidationResult Validate(ResendInvitationCommand item)
    {
        var validationResult = new ValidationResult();

        if (string.IsNullOrWhiteSpace(item.HashedInvitationId))
        {
            validationResult.AddError("HashedInvitationId", "No HashedInvitationId supplied");
        }

        if (string.IsNullOrEmpty(item.HashedAccountId))
        {
            validationResult.AddError("HashedId", "No HashedId supplied");
        }

        if (string.IsNullOrWhiteSpace(item.ExternalUserId))
        {
            validationResult.AddError("ExternalUserId", "No ExternalUserId supplied");
        }

        return validationResult;
    }

    public Task<ValidationResult> ValidateAsync(ResendInvitationCommand item)
    {
        return Task.FromResult(Validate(item));
    }
}