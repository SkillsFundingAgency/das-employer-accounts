using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.Accounts;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Responses.Accounts;
using SFA.DAS.EmployerAccounts.Interfaces.OuterApi;
using SFA.DAS.EmployerAccounts.Models.Account;

namespace SFA.DAS.EmployerAccounts.Services;

public class EmployerAccountService(
    IOuterApiClient apiClient,
    ILogger<EmployerAccountService> logger) : IEmployerAccountService
{
    public async Task<TaskSummary> GetTaskSummary(long accountId)
    {
        TaskSummary taskSummary = null;

        try
        {
            logger.LogInformation("Getting TaskSummary for account ID: {accountId}", accountId);

            var tasksResponse = await apiClient.Get<GetTasksResponse>(new GetTasksRequest(accountId));

            if (tasksResponse != null)
            {
                taskSummary =  MapFrom(tasksResponse);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not find TaskSummary for account ID: {accountId}", accountId);
            return new TaskSummary
            {
                UnableToGetTasks = true
            };
        }

        return taskSummary;
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