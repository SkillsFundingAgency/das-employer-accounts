namespace SFA.DAS.EmployerAccounts.Queries.HasCompletedLevyDormancyRequest;

public class HasCompletedLevyDormancyRequestQuery : IRequest<HasCompletedLevyDormancyRequestResponse>
{
    public long AccountId { get; set; }
}
