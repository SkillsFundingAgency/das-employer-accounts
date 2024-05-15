namespace SFA.DAS.EmployerAccounts.Commands.SupportResendInvitationCommand;

public class SupportResendInvitationCommand : IRequest
{
    public string Email { get; set; }
    public string HashedAccountId { get; set; }
    public string FirstName { get; set; }
}