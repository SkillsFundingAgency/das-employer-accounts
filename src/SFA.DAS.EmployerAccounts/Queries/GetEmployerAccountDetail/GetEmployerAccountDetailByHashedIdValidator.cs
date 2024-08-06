namespace SFA.DAS.EmployerAccounts.Queries.GetEmployerAccountDetail;

public class GetEmployerAccountDetailByHashedIdValidator : IValidator<GetEmployerAccountDetailByIdQuery>
{
    public ValidationResult Validate(GetEmployerAccountDetailByIdQuery item)
    {
        var validationResult = new ValidationResult();

        if (item.AccountId <= 0)
        {
            validationResult.AddError(nameof(item.AccountId), "AccountId has not been supplied");
        }

        return validationResult;
    }

    public Task<ValidationResult> ValidateAsync(GetEmployerAccountDetailByIdQuery item)
    {
        return Task.FromResult(Validate(item));
    }
}