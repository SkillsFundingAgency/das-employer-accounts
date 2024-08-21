using SFA.DAS.EmployerAccounts.Models.PAYE;

namespace SFA.DAS.EmployerAccounts.Queries.GetPayeSchemeAccountByRef;

public class GetPayeSchemeAccountByRefQuery : IRequest<PayeScheme>
{
    public string Ref { get; set; }
}