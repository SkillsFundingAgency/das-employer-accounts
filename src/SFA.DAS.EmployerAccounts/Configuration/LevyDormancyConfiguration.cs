namespace SFA.DAS.EmployerAccounts.Configuration;

public class LevyDormancyConfiguration
{
    public bool AssessmentEnabled { get; set; }

    // Minimum months since last positive net levy declaration before an account is treated as dormant.
    public int DormancyDetectionMonths { get; set; } = 21;

    // Used by the LevyDormancyRequest orchestration process (not the assessment job).
    public int InitialWarningMonths { get; set; } = 21;

    public int SwitchMonths { get; set; } = 24;

    public int MonthsBetweenInitialWarningAndSwitch { get; set; } = 3;

    public string LevyStatusAssessmentJobSchedule { get; set; } = "0 0 6 1 * *";
}
