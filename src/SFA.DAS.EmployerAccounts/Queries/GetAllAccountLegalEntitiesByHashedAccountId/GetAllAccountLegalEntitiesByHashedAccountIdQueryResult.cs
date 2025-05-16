using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Models.Account;

namespace SFA.DAS.EmployerAccounts.Queries.GetAllAccountLegalEntitiesByHashedAccountId
{
    public record GetAllAccountLegalEntitiesByHashedAccountIdQueryResult
    {
        public PaginatedList<AccountLegalEntity> LegalEntities { get; set; }
    }
}