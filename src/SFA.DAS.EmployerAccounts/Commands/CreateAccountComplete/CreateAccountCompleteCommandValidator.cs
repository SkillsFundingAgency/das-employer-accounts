namespace SFA.DAS.EmployerAccounts.Commands.CreateAccountComplete;

public class CreateAccountCompleteCommandValidator : IValidator<CreateAccountCompleteCommand>
{
    public ValidationResult Validate(CreateAccountCompleteCommand command)
    {
        var validationResult = new ValidationResult();
        
        if (string.IsNullOrWhiteSpace(command.HashedAccountId))
        {
            validationResult.AddError(nameof(command.HashedAccountId), "No HashedAccountId supplied");
        }
        
        if (string.IsNullOrWhiteSpace(command.OrganisationName))
        {
            validationResult.AddError(nameof(command.OrganisationName), "No OrganisationName supplied");
        }
        
        if (string.IsNullOrWhiteSpace(command.ExternalUserId))
        {
            validationResult.AddError(nameof(command.ExternalUserId), "No ExternalUserId supplied");
        }

        return validationResult;
    }

    public Task<ValidationResult> ValidateAsync(CreateAccountCompleteCommand query)
    {
        throw new NotImplementedException();
    }
}