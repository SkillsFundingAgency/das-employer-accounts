﻿using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.UserAccounts;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Responses.UserAccounts;
using SFA.DAS.EmployerAccounts.Interfaces.OuterApi;
using SFA.DAS.GovUK.Auth.Employer;
using EmployerUserAccounts = SFA.DAS.EmployerAccounts.Models.UserAccounts.EmployerUserAccounts;

namespace SFA.DAS.EmployerAccounts.Services;

public class UserAccountService : IGovAuthEmployerAccountService
{
    private readonly IOuterApiClient _outerApiClient;

    public UserAccountService(IOuterApiClient outerApiClient)
    {
        _outerApiClient = outerApiClient;
    }

    async Task<GovUK.Auth.Employer.EmployerUserAccounts> IGovAuthEmployerAccountService.GetUserAccounts(string userId, string email)
    {
        var result = await _outerApiClient.Get<GetUserAccountsResponse>(new GetUserAccountsRequest(email, userId));

        return new GovUK.Auth.Employer.EmployerUserAccounts
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
