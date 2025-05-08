namespace SFA.DAS.EmployerAccounts.Queries.SearchEmployerAccountsByName;

public class SearchEmployerAccountsByNameQueryValidator : IValidator<SearchEmployerAccountsByNameQuery>
{
    public ValidationResult Validate(SearchEmployerAccountsByNameQuery item)
    {
        var validationResult = new ValidationResult();

        if (string.IsNullOrWhiteSpace(item.EmployerName))
        {
            validationResult.AddError(nameof(item.EmployerName), "Employer name has not been supplied");
        }

        return validationResult;
    }

    public Task<ValidationResult> ValidateAsync(SearchEmployerAccountsByNameQuery item)
    {
        return Task.FromResult(Validate(item));
    }
} 