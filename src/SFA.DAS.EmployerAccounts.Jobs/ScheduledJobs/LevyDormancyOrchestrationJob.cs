using MediatR;
using SFA.DAS.EmployerAccounts.Commands.LevyDormancy;

namespace SFA.DAS.EmployerAccounts.Jobs.ScheduledJobs;

public class LevyDormancyOrchestrationJob(IMediator mediator)
{
    public async Task Run([TimerTrigger("%SFA.DAS.EmployerAccounts:LevyDormancy:LevyDormancyOrchestrationJobSchedule%", RunOnStartup = false)] TimerInfo timer, ILogger logger)
    {
        logger.LogInformation("Starting {JobName}", nameof(LevyDormancyOrchestrationJob));

        var warningResult = await mediator.Send(new ProcessLevyDormancyWarningsCommand());

        logger.LogInformation(
            "{JobName} warnings completed. Processed {RequestsProcessed}, emails sent {EmailsSent}, cancelled {RequestsCancelled}, skipped no recipients {SkippedNoRecipients}, skipped not yet eligible {SkippedNotYetEligible}",
            nameof(LevyDormancyOrchestrationJob),
            warningResult.RequestsProcessed,
            warningResult.EmailsSent,
            warningResult.RequestsCancelled,
            warningResult.SkippedNoRecipients,
            warningResult.SkippedNotYetEligible);

        var switchResult = await mediator.Send(new ProcessLevyDormancySwitchesCommand());

        logger.LogInformation(
            "{JobName} switches completed. Processed {RequestsProcessed}, accounts switched {AccountsSwitched}, emails sent {EmailsSent}, cancelled {RequestsCancelled}, skipped not yet eligible {SkippedNotYetEligible}, skipped no recipients {SkippedNoRecipients}",
            nameof(LevyDormancyOrchestrationJob),
            switchResult.RequestsProcessed,
            switchResult.AccountsSwitched,
            switchResult.EmailsSent,
            switchResult.RequestsCancelled,
            switchResult.SkippedNotYetEligible,
            switchResult.SkippedNoRecipients);
    }
}
