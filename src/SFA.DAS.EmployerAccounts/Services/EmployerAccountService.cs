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
    
    public async Task<EmployerAccountTaskList> GetEmployerAccountTaskList(long accountId, string hashedAccountId)
    {
        EmployerAccountTaskList taskList = null;

        try
        {
            _logger.LogInformation("Getting task list for account ID: {hashedAccountId}", hashedAccountId);

            var taskListResponse = await _outerApiClient.Get<GetEmployerAccountTaskListResponse>(new GetEmployerAccountTaskListRequest(accountId, hashedAccountId));

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

    public async Task<TaskSummary> GetTaskSummary(long accountId)
    {
        TaskSummary taskSummary = null;

        try
        {
            _logger.LogInformation("Getting TaskSummary for account ID: {accountId}", accountId);

            var tasksResponse = await _outerApiClient.Get<GetTasksResponse>(new GetTasksRequest(accountId));

            if (tasksResponse != null)
            {
                taskSummary =  MapFrom(tasksResponse);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not find TaskSummary for account ID: {accountId}", accountId);
            return new TaskSummary
            {
                UnableToGetTasks = true
            };
        }

        return taskSummary;
    }
    
    private static EmployerAccountTaskList MapFrom(GetEmployerAccountTaskListResponse getEmployerTaskListResponse)
    {
        return new EmployerAccountTaskList
        {
            HasProviders = getEmployerTaskListResponse.HasProviders,
            HasProviderPermissions = getEmployerTaskListResponse.HasPermissions
        };
    } 
    
    private static TaskSummary MapFrom(GetTasksResponse getTasksResponse)
    {
        return new TaskSummary
        {
          ShowLevyDeclarationTask = getTasksResponse.ShowLevyDeclarationTask,
          NumberOfApprenticesToReview = getTasksResponse.NumberOfApprenticesToReview,
          NumberOfCohortsReadyToReview  = getTasksResponse.NumberOfCohortsReadyToReview,
          NumberOfPendingTransferConnections = getTasksResponse.NumberOfPendingTransferConnections,
          NumberOfTransferRequestToReview = getTasksResponse.NumberOfTransferRequestToReview,
          NumberTransferPledgeApplicationsToReview = getTasksResponse.NumberTransferPledgeApplicationsToReview
        };
    }
}