namespace SFA.DAS.EmployerAccounts.Configuration;

public class LevyDormancyConfiguration
{
    public bool AssessmentEnabled { get; set; }

    public bool OrchestrationEnabled { get; set; }

    // When to create a LevyDormancyRequest (monthly assessment). Default 20 absorbs assessment lag before the 21-month warning.
    public int DormancyDetectionMonths { get; set; } = 20;

    // When orchestration may send the initial warning email (monthly job).
    public int InitialWarningMonths { get; set; } = 21;

    public int SwitchMonths { get; set; } = 24;

    public int MonthsBetweenInitialWarningAndSwitch { get; set; } = 3;
}
