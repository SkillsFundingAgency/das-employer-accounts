using SFA.DAS.EmployerAccounts.Commands.ChangeTeamMemberRole;

namespace SFA.DAS.EmployerAccounts.Commands.SupportChangeTeamMemberRole;

public class SupportChangeTeamMemberRoleCommandValidator : IValidator<SupportChangeTeamMemberRoleCommand>
{
    public ValidationResult Validate(SupportChangeTeamMemberRoleCommand item)
    {
        var validationResult = new ValidationResult();

        if (string.IsNullOrEmpty(item.HashedAccountId))
        {
            validationResult.AddError("HashedId", "No HashedId supplied");
        }

        if (string.IsNullOrWhiteSpace(item.Email))
        {
            validationResult.AddError("Email", "No Email supplied");
        }

        if (string.IsNullOrWhiteSpace(item.SupportUserEmail))
        {
            validationResult.AddError("SupportUserEmail", "No SupportUserEmail supplied");
        }

        return validationResult;
    }

    public Task<ValidationResult> ValidateAsync(SupportChangeTeamMemberRoleCommand item)
    {
        return Task.FromResult(Validate(item));
    }
}