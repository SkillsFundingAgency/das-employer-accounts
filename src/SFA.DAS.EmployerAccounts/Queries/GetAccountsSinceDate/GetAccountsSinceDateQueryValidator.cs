namespace SFA.DAS.EmployerAccounts.Queries.GetAccountsSinceDate;

public class GetAccountsSinceDateQueryValidator : IValidator<GetAccountsSinceDateQuery>
{
    public ValidationResult Validate(GetAccountsSinceDateQuery query)
    {
        throw new NotImplementedException();
    }

    public Task<ValidationResult> ValidateAsync(GetAccountsSinceDateQuery query)
    {
        var validationResult = new ValidationResult();
        if (query.PageNumber <= 0)
        {
            validationResult.AddError(nameof(query.PageNumber), "Page number must be greater than zero when provided");
        }

        if (query.PageSize <= 0)
        {
            validationResult.AddError(nameof(query.PageSize), "Page size must be greater than zero when provided");   
        }

        return Task.FromResult(validationResult);
    }
}
