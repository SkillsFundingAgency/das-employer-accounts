using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.UserAccounts;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Responses.UserAccounts;
using SFA.DAS.EmployerAccounts.Interfaces.OuterApi;
using SFA.DAS.GovUK.Auth.Employer;

namespace SFA.DAS.EmployerAccounts.Services;

public class UserAccountService(IOuterApiClient outerApiClient) : IGovAuthEmployerAccountService
{
    async Task<EmployerUserAccounts> IGovAuthEmployerAccountService.GetUserAccounts(string userId, string email)
    {
        var result = await outerApiClient.Get<GetUserAccountsResponse>(new GetUserAccountsRequest(email, userId));

        return new EmployerUserAccounts
        {
            EmployerAccounts = result.UserAccounts != null? result.UserAccounts.Select(c => new EmployerUserAccountItem
            {
                Role = c.Role,
                AccountId = c.AccountId,
                ApprenticeshipEmployerType = Enum.Parse<ApprenticeshipEmployerType>(c.ApprenticeshipEmployerType.ToString()),
                EmployerName = c.EmployerName,
            }).ToList() : [],
            FirstName = result.FirstName,
            IsSuspended = result.IsSuspended,
            LastName = result.LastName,
            EmployerUserId = result.EmployerUserId,
        };
    }
}
