
namespace SFA.DAS.EmployerAccounts.Queries.GetAccounts
{
    public class GetAccountsQueryValidator : IValidator<GetAccountsQuery>
    {
        public ValidationResult Validate(GetAccountsQuery query)
        {
            throw new NotImplementedException();
        }

        public Task<ValidationResult> ValidateAsync(GetAccountsQuery query)
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
}
