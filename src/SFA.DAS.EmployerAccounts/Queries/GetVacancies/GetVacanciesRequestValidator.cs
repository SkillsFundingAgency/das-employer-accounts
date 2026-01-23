namespace SFA.DAS.EmployerAccounts.Queries.GetVacancies;

public class GetVacanciesRequestValidator : IValidator<GetVacanciesRequest>
{
    public ValidationResult Validate(GetVacanciesRequest item)
    {
        var validationResult = new ValidationResult();

        if (item.AccountId == 0)
        {
            validationResult.ValidationDictionary.Add(nameof(item.AccountId),
                "Account Id must be set.");
        }

        return validationResult;
    }

    public Task<ValidationResult> ValidateAsync(GetVacanciesRequest item)
    {
        return Task.FromResult(Validate(item));
    }
}