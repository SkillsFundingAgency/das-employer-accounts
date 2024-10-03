using SFA.DAS.EmployerAccounts.Models.PAYE;

namespace SFA.DAS.EmployerAccounts.Queries.GetAccountHistoryByPayeRef;

public class GetAccountHistoryByPayeRefQuery : IRequest<PayeScheme>
{
    public string Ref { get; set; }
}