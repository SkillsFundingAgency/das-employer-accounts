using MediatR;
using SFA.DAS.EmployerAccounts.Commands.LevyDormancy;

namespace SFA.DAS.EmployerAccounts.Jobs.ScheduledJobs;

public class LevyDormancyOrchestrationJob(IMediator mediator)
{
    public async Task Run([TimerTrigger("%SFA.DAS.EmployerAccounts:LevyDormancy:LevyDormancyOrchestrationJobSchedule%", RunOnStartup = false)] TimerInfo timer, ILogger logger)
    {
        logger.LogInformation("Starting {JobName}", nameof(LevyDormancyOrchestrationJob));

        var result = await mediator.Send(new ProcessLevyDormancyWarningsCommand());

        logger.LogInformation(
            "{JobName} completed. Processed {RequestsProcessed}, emails sent {EmailsSent}, cancelled {RequestsCancelled}, skipped no recipients {SkippedNoRecipients}, skipped not yet eligible {SkippedNotYetEligible}",
            nameof(LevyDormancyOrchestrationJob),
            result.RequestsProcessed,
            result.EmailsSent,
            result.RequestsCancelled,
            result.SkippedNoRecipients,
            result.SkippedNotYetEligible);
    }
}
