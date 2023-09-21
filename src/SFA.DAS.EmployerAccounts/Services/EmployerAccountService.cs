using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.Reservations;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Responses.Reservations;
using SFA.DAS.EmployerAccounts.Interfaces.OuterApi;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Models.Reservations;
using ReservationStatus = SFA.DAS.EmployerAccounts.Models.Reservations.ReservationStatus;

namespace SFA.DAS.EmployerAccounts.Services;

public class EmployerAccountService : IEmployerAccountService
{
    private readonly IOuterApiClient _outerApiClient;
    private readonly ILogger<EmployerAccountService> _logger;
    public EmployerAccountService(IOuterApiClient apiClient, ILogger<EmployerAccountService> logger)
    {
        _logger = logger;
        _outerApiClient = apiClient;
    }
    
    public Task<EmployerAccountTaskList> GetEmployerAccountTaskList(long accountId)
    {
        throw new NotImplementedException();
    }
}