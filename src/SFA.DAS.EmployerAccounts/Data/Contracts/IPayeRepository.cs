using System.Threading;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Models.PAYE;
using SFA.DAS.EmployerAccounts.Queries.GetPayeAccountByRef;

namespace SFA.DAS.EmployerAccounts.Data.Contracts;

public interface IPayeRepository
{
    Task<List<PayeView>> GetPayeSchemesByAccountId(long accountId);
    Task AddPayeToAccount(Paye payeScheme);
    Task RemovePayeFromAccount(long accountId, string payeRef);
    Task<PayeSchemeView> GetPayeForAccountByRef(long accountId, string reference);

    Task<GetPayeAccountByRefResponse> GetPayeAccountByRef(string reference, CancellationToken cancellationToken);
}