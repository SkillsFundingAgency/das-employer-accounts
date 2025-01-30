namespace SFA.DAS.EmployerAccounts.Models.Account;

public class TaskSummary
{
    public bool ShowLevyDeclarationTask { get; set; }
    public int NumberOfApprenticesToReview { get; set; }
    public int NumberOfCohortsReadyToReview { get; set; }
    public int NumberOfPendingTransferConnections { get; set; }
    public int NumberOfTransferRequestToReview { get; set; }
    public int NumberTransferPledgeApplicationsToReview { get; set; }
    public int NumberOfTransferPledgeApplicationsApproved { get; set; }
    public int? SingleApprovedTransferApplicationId { get; set; }
    public string SingleApprovedTransferApplicationHashedId { get; set; }
    public bool UnableToGetTasks { get; set; }

    public bool HasAnyTask()
    {
        return ShowLevyDeclarationTask ||
               Array.Exists([
                   NumberOfApprenticesToReview,
                   NumberOfCohortsReadyToReview,
                   NumberOfPendingTransferConnections,
                   NumberOfTransferRequestToReview,
                   NumberTransferPledgeApplicationsToReview,
                   NumberOfTransferPledgeApplicationsApproved
               ], x => x > 0);
    }
}