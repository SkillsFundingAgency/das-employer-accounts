namespace SFA.DAS.EmployerAccounts.Queries.GetAccountLegalEntitiesByHashedAccountId;

public class GetAccountLegalEntitiesByHashedAccountIdValidator : IValidator<GetAccountLegalEntitiesByHashedAccountIdRequest>
{
    public ValidationResult Validate(GetAccountLegalEntitiesByHashedAccountIdRequest item)
    {
        var validationResult = new ValidationResult();

        if (item.AccountId <= 0)
        {
            validationResult.AddError(nameof(item.AccountId), "AccountId has not been supplied");
        }

        return validationResult;
    }

    public Task<ValidationResult> ValidateAsync(GetAccountLegalEntitiesByHashedAccountIdRequest item)
    {
        return Task.FromResult(Validate(item));
    }
}