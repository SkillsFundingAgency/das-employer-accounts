using System;

namespace SFA.DAS.EmployerAccounts.Messages.Events;

public class LevyDormancyThresholdReachedEvent
{
    public long AccountId { get; set; }
    public int NoLevyDeclaredMonths { get; set; }
    public DateTime? LastLevyDeclarationDate { get; set; }
    public DateTime AssessedOn { get; set; }
    public DateTime Created { get; set; }
}