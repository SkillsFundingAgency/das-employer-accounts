﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerApprenticeshipsService.Domain.Data
{
    public interface IAccountTeamRepository
    {
        Task<List<TeamMember>> GetAccountTeamMembersForUserId(int accountId, string externalUserId);
        Task<TeamMember> GetMember(long accountId, string email);
    }
}
