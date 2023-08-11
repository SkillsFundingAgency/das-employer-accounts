namespace SFA.DAS.EmployerAccounts.Commands.CreateAccountComplete;

public record CreateAccountCompleteCommand : IRequest
{
    public string HashedAccountId { get; set; }
    public long AccountId { get; set; }
    public string PublicHashedAccountId { get; set; }
    public string OrganisationName { get; set; }
    public string ExternalUserId { get; set; }
}