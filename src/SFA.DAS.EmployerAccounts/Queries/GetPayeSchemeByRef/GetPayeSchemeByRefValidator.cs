namespace SFA.DAS.EmployerAccounts.Queries.GetPayeSchemeByRef;

public class GetPayeSchemeByRefValidator : IValidator<GetPayeSchemeByRefQuery>
{
    public ValidationResult Validate(GetPayeSchemeByRefQuery item)
    {
        var validationResult = new ValidationResult();

        if (item.AccountId <= 0)
        {
            validationResult.AddError(nameof(item.AccountId), "AccountId has not been supplied");
        }

        if (string.IsNullOrEmpty(item.Ref))
        {
            validationResult.AddError(nameof(item.Ref), "PayeSchemeRef has not been supplied");
        }
        return validationResult;
    }

    public Task<ValidationResult> ValidateAsync(GetPayeSchemeByRefQuery item)
    {
        return Task.FromResult(Validate(item));
    }
}