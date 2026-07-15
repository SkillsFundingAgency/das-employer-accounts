namespace SFA.DAS.EmployerAccounts.Commands.LevyDormancy;

public class ProcessLevyDormancyWarningsCommand : IRequest<ProcessLevyDormancyWarningsResult>;

public class ProcessLevyDormancyWarningsResult
{
    public int RequestsProcessed { get; set; }

    public int EmailsSent { get; set; }

    public int RequestsCancelled { get; set; }

    public int SkippedNoRecipients { get; set; }

    public int SkippedNotYetEligible { get; set; }
}
