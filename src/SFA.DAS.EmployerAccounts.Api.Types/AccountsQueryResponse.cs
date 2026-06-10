using System.Collections.Generic;

namespace SFA.DAS.EmployerAccounts.Api.Types
{
    public class AccountsQueryResponse
    {
        public List<AccountQueryResult> Accounts { get; set; } = new List<AccountQueryResult>();
    }

    public class AccountQueryResult
    {
        public long AccountId { get; set; }
        public string ApprenticeshipEmployerType { get; set; }
        public List<AccountQueryLegalEntityResult> LegalEntities { get; set; } = new List<AccountQueryLegalEntityResult>();
    }

    public class AccountQueryLegalEntityResult
    {
        public string Id { get; set; }
    }
}
