namespace SFA.DAS.EmployerAccounts.Commands.CreateAccountComplete;

public record CreateAccountCompleteCommand : IRequest<CreateAccountCompleteCommand>, IRequest<Unit>
{
    public string HashedAccountId { get; init; }
    public string OrganisationName { get; init; }
    public string ExternalUserId { get; init; }
}