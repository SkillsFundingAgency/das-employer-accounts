using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerAccounts.Api.Types;
using SFA.DAS.EmployerAccounts.Queries.GetUserAccounts;

namespace SFA.DAS.EmployerAccounts.Api.Orchestrators;

public interface IUsersOrchestrator
{
    Task<ICollection<AccountDetail>> GetUserAccounts(string userRef);
}

public class UsersOrchestrator(IMediator mediator, ILogger<UsersOrchestrator> logger, IMapper mapper) : IUsersOrchestrator
{
    public async Task<ICollection<AccountDetail>> GetUserAccounts(string userRef)
    {
        logger.LogInformation("Requesting user's accounts for user Ref {UserRef}", userRef);

        var accounts = await mediator.Send(new GetUserAccountsQuery { UserRef = userRef });

        return accounts.Accounts.AccountList.Select(mapper.Map<AccountDetail>).ToList();
    }
}