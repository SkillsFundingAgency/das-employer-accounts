namespace SFA.DAS.EmployerAccounts.Queries.GetTaskSummary
{
    public class GetTaskSummaryQueryValidator : IValidator<GetTaskSummaryQuery>
    {
        public ValidationResult Validate(GetTaskSummaryQuery item)
        {
            var validationResult = new ValidationResult();
            if (item == null)
            {
                validationResult.AddError(nameof(GetTaskSummaryQuery), "Message must be supplied");
                return validationResult;
            }
            if (item.AccountId == default(int))
            {
                validationResult.AddError(nameof(item.AccountId), "Account id must be supplied");
            }

            return validationResult;
        }

        public Task<ValidationResult> ValidateAsync(GetTaskSummaryQuery item)
        {
            return Task.FromResult(Validate(item));
        }
    }
}
