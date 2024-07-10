namespace SFA.DAS.EmployerAccounts.Data.Contracts;

public interface IEmployerAccountTeamRepository
{
    Task<List<TeamMember>> GetAccountTeamMembersForUserId(string hashedAccountId, string externalUserId);
    Task<TeamMember> GetMember(string hashedAccountId, string email, bool onlyIfMemberIsActive);
    Task<TeamMember> GetMember(string hashedAccountId, long id, MemberType memberType);
    Task<List<TeamMember>> GetAccountTeamMembers(long accountId);
}