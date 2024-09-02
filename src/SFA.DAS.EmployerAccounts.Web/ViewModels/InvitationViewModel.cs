using SFA.DAS.EmployerAccounts.Models.AccountTeam;

namespace SFA.DAS.EmployerAccounts.Web.ViewModels;

public class InvitationViewModel : IAccountIdentifier
{
    public bool IsUser { get; set; }
    public long Id { get; set; }
    public long AccountId { get; set; }
    public string HashedAccountId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public Role Role { get; set; }
    public InvitationStatus Status { get; set; }
    public DateTime ExpiryDate { get; set; }   
    
    public static InvitationViewModel MapFrom(TeamMember teamMember)
    {
        return new InvitationViewModel
        {
            IsUser = teamMember.IsUser,
            Id = teamMember.Id,
            AccountId = teamMember.AccountId,
            Email = teamMember.Email,
            Name = teamMember.Name,
            Role = teamMember.Role,
            Status = teamMember.Status,
            ExpiryDate = teamMember.ExpiryDate,
            HashedAccountId = teamMember.HashedAccountId
        };
    }
}