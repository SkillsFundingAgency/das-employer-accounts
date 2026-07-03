namespace SFA.DAS.EmployerAccounts.Commands.LevyDormancy;

public class AssessLevyDormancyCommand : IRequest<AssessLevyDormancyResult>;

public class AssessLevyDormancyResult
{
    public int AccountsAssessed { get; set; }

    public int DormantCandidatesFound { get; set; }

    public int DormancyRequestsCreated { get; set; }
}
