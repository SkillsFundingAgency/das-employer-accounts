namespace SFA.DAS.EmployerAccounts.Configuration;

public class LevyDormancyConfiguration
{
    public bool AssessmentEnabled { get; set; }

    public int DormancyDetectionMonths { get; set; } = 21;

    public int InitialWarningMonths { get; set; } = 21;

    public int SwitchMonths { get; set; } = 24;

    public int MonthsBetweenInitialWarningAndSwitch { get; set; } = 3;

    public string LevyStatusAssessmentJobSchedule { get; set; } = "0 0 6 1 * *";

    public string IgnoredAccountIds { get; set; } = string.Empty;

    public IReadOnlySet<long> GetIgnoredAccountIds()
    {
        if (string.IsNullOrWhiteSpace(IgnoredAccountIds))
        {
            return new HashSet<long>();
        }

        var ignored = new HashSet<long>();

        foreach (var token in IgnoredAccountIds.Split(','))
        {
            if (long.TryParse(token.Trim(), out var accountId))
            {
                ignored.Add(accountId);
            }
        }

        return ignored;
    }
}
