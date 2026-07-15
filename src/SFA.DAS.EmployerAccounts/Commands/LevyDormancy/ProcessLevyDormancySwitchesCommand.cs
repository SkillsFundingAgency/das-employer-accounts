namespace SFA.DAS.EmployerAccounts.Commands.LevyDormancy;

public class ProcessLevyDormancySwitchesCommand : IRequest<ProcessLevyDormancySwitchesResult>;

public class ProcessLevyDormancySwitchesResult
{
    public int RequestsProcessed { get; set; }

    public int AccountsSwitched { get; set; }

    public int EmailsSent { get; set; }

    public int RequestsCancelled { get; set; }

    public int SkippedNotYetEligible { get; set; }

    public int SkippedNoRecipients { get; set; }
}
