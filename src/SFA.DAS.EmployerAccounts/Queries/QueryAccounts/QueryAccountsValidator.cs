namespace SFA.DAS.EmployerAccounts.Queries.QueryAccounts;

public class QueryAccountsValidator : IValidator<QueryAccountsRequest>
{
    public ValidationResult Validate(QueryAccountsRequest item)
    {
        var result = new ValidationResult();

        if (item.AccountIds is null || item.AccountIds.Count == 0)
        {
            result.AddError(nameof(item.AccountIds), "At least one account ID must be supplied");
            return result;
        }

        if (item.AccountIds.Count > QueryAccountsRequest.MaxAccountIds)
        {
            result.AddError(nameof(item.AccountIds), $"A maximum of {QueryAccountsRequest.MaxAccountIds} account IDs can be supplied");
        }

        if (item.AccountIds.Any(id => id <= 0))
        {
            result.AddError(nameof(item.AccountIds), "All account IDs must be greater than zero");
        }

        if (item.Select is not null)
        {
            foreach (var field in item.Select)
            {
                if (!IsSupportedSelectField(field))
                {
                    result.AddError(nameof(item.Select), $"'{field}' is not a supported select field");
                }
            }
        }

        if (item.Include is not null)
        {
            foreach (var include in item.Include)
            {
                if (!IsSupportedInclude(include))
                {
                    result.AddError(nameof(item.Include), $"'{include}' is not a supported include value");
                }
            }
        }

        return result;
    }

    public Task<ValidationResult> ValidateAsync(QueryAccountsRequest item)
    {
        return Task.FromResult(Validate(item));
    }

    private static bool IsSupportedSelectField(string field)
    {
        return string.Equals(field, AccountQueryFields.ApprenticeshipEmployerType, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSupportedInclude(string include)
    {
        return string.Equals(include, AccountQueryFields.LegalEntities, StringComparison.OrdinalIgnoreCase);
    }
}
