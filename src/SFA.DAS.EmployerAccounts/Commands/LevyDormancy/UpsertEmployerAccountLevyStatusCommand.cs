namespace SFA.DAS.EmployerAccounts.Commands.LevyDormancy;

public class UpsertEmployerAccountLevyStatusCommand : IRequest
{
    public long AccountId { get; set; }
    public DateTime? LastLevyDeclarationDate { get; set; }
    public DateTime RefreshedAt { get; set; }
}
