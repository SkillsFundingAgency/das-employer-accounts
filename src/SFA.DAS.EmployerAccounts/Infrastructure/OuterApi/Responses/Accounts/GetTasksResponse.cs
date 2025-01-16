﻿namespace SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Responses.Accounts;

public class GetTasksResponse
{
    public bool ShowLevyDeclarationTask { get; set; }
    public int NumberOfApprenticesToReview { get; set; }
    public int NumberOfCohortsReadyToReview { get; set; }
    public int NumberOfPendingTransferConnections { get; set; }
    public int NumberOfTransferRequestToReview { get; set; }
    public int NumberTransferPledgeApplicationsToReview { get; set; }
    public int NumberOfTransferPledgeApplicationsApproved { get; set; }
    public int? SingleApprovedTransferPledgeId { get; set; }
}