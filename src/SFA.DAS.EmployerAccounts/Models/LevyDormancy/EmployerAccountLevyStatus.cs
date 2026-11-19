namespace SFA.DAS.EmployerAccounts.Models.LevyDormancy;

public class EmployerAccountLevyStatus
{
    public long Id { get; set; }
    public long AccountId { get; set; }
    public DateTime? LastLevyDeclarationDate { get; set; }
    public DateTime LastRefreshedAt { get; set; }
}

public class LevyDormancyRequest
{
    public long Id { get; set; }
    public long AccountId { get; set; }
    public int NoLevyDeclaredMonths { get; set; }
    public DateTime? LastLevyDeclarationDate { get; set; }
    public LevyDormancyRequestStatus Status { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public DateTime? WarningEmailSentAt { get; set; }
    public DateTime? FinalWarningEmailSentAt { get; set; }
    public DateTime? ActionEmailSentAt { get; set; }
}

public enum LevyDormancyRequestStatus : byte
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}
