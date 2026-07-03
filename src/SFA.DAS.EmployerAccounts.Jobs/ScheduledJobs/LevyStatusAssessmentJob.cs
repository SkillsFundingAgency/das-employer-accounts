using MediatR;
using SFA.DAS.EmployerAccounts.Commands.LevyDormancy;

namespace SFA.DAS.EmployerAccounts.Jobs.ScheduledJobs;

public class LevyStatusAssessmentJob(IMediator mediator)
{
    public async Task Run([TimerTrigger("0 0 6 1 * *")] TimerInfo timer, ILogger logger)
    {
        logger.LogInformation("Starting {JobName}", nameof(LevyStatusAssessmentJob));

        var result = await mediator.Send(new AssessLevyDormancyCommand());

        logger.LogInformation(
            "{JobName} completed. Assessed {AccountsAssessed}, dormant candidates {DormantCandidatesFound}, dormancy requests created {DormancyRequestsCreated}",
            nameof(LevyStatusAssessmentJob),
            result.AccountsAssessed,
            result.DormantCandidatesFound,
            result.DormancyRequestsCreated);
    }
}
