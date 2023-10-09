using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.Accounts;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Responses.Accounts;
using SFA.DAS.EmployerAccounts.Interfaces.OuterApi;
using SFA.DAS.EmployerAccounts.Models.Account;

namespace SFA.DAS.EmployerAccounts.Services;

public class EmployerAccountService : IEmployerAccountService
{
    private readonly IOuterApiClient _outerApiClient;
    private readonly ILogger<EmployerAccountService> _logger;
    public EmployerAccountService(IOuterApiClient apiClient, ILogger<EmployerAccountService> logger)
    {
        _logger = logger;
        _outerApiClient = apiClient;
    }
    
    public async Task<EmployerAccountTaskList> GetEmployerAccountTaskList(string hashedAccountId)
    {
        EmployerAccountTaskList taskList = null;

        try
        {
            _logger.LogInformation("Getting task list for account ID: {hashedAccountId}", hashedAccountId);

            var taskListResponse = await _outerApiClient.Get<GetEmployerAccountTaskListResponse>(new GetEmployerAccountTaskListRequest(hashedAccountId));

            if (taskListResponse != null)
            {
                taskList =  MapFrom(taskListResponse);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not find employer account task list for account ID: {hashedAccountId}", hashedAccountId);
        }

        return taskList;
    }
    
    private static EmployerAccountTaskList MapFrom(GetEmployerAccountTaskListResponse getEmployerTaskListResponse)
    {
        return new EmployerAccountTaskList
        {
            HasProviders = getEmployerTaskListResponse.HasProviders,
            HasProviderPermissions = getEmployerTaskListResponse.HasPermissions
        };
    }
}