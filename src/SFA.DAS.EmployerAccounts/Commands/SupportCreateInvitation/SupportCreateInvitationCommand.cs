using SFA.DAS.EmployerAccounts.Models;

namespace SFA.DAS.EmployerAccounts.Commands.SupportCreateInvitation;

public class SupportCreateInvitationCommand : IRequest
{
    public string HashedAccountId { get; set; }

    public string NameOfPersonBeingInvited { get; set; }

    public string EmailOfPersonBeingInvited { get; set; }

    public Role RoleOfPersonBeingInvited { get; set; }
}