namespace SFA.DAS.EmployerAccounts.Queries.GetAccountById
{
    public class GetAccountByIdValidator : IValidator<GetAccountByIdQuery>
    {
        public GetAccountByIdValidator()
        {
        }

        public ValidationResult Validate(GetAccountByIdQuery item)
        {
            var result = new ValidationResult();

            if (item.AccountId <= 0)
            {
                result.AddError(nameof(item.AccountId), "Account ID has not been supplied");
            }

            return result;
        }

        public Task<ValidationResult> ValidateAsync(GetAccountByIdQuery item)
        {
            return Task.FromResult(Validate(item));
        }
    }
}