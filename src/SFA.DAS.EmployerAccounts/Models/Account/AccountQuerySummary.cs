using SFA.DAS.Common.Domain.Types;

namespace SFA.DAS.EmployerAccounts.Models.Account;

public class AccountQuerySummary
{
    public long AccountId { get; set; }
    public ApprenticeshipEmployerType ApprenticeshipEmployerType { get; set; }
    public List<long> LegalEntityIds { get; set; } = [];
}
