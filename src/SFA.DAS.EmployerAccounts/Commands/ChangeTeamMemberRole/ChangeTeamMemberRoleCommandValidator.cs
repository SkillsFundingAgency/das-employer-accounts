namespace SFA.DAS.EmployerAccounts.Commands.ChangeTeamMemberRole;

public class ChangeTeamMemberRoleCommandValidator : IValidator<ChangeTeamMemberRoleCommand>
{
    public ValidationResult Validate(ChangeTeamMemberRoleCommand item)
    {
        var validationResult = new ValidationResult();

        if (string.IsNullOrEmpty(item.HashedAccountId))
        {
            validationResult.AddError(nameof(ChangeTeamMemberRoleCommand.HashedAccountId), $"No {nameof(ChangeTeamMemberRoleCommand.HashedAccountId)} supplied");
        }

        if (string.IsNullOrEmpty(item.HashedUserId))
        {
            validationResult.AddError(nameof(ChangeTeamMemberRoleCommand.HashedUserId), $"No {nameof(ChangeTeamMemberRoleCommand.HashedUserId)} supplied");
        }

        if (string.IsNullOrWhiteSpace(item.ExternalUserId))
        {
            validationResult.AddError(nameof(ChangeTeamMemberRoleCommand.ExternalUserId), $"No {nameof(ChangeTeamMemberRoleCommand.ExternalUserId)} supplied");
        }

        return validationResult;
    }

    public Task<ValidationResult> ValidateAsync(ChangeTeamMemberRoleCommand item)
    {
        return Task.FromResult(Validate(item));
    }
}