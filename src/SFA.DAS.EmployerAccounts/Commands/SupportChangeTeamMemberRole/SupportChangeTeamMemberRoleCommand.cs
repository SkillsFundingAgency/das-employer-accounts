using SFA.DAS.EmployerAccounts.Models;

namespace SFA.DAS.EmployerAccounts.Commands.SupportChangeTeamMemberRole;

public class SupportChangeTeamMemberRoleCommand : IRequest
{
    public string HashedAccountId { get; set; }
    public string Email { get; set; }
    public string SupportUserEmail { get; set; }
    public Role Role { get; set; }
}