using SFA.DAS.EmployerAccounts.Interfaces.OuterApi;

namespace SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.Vacancies;

public class GetVacanciesApiRequest(long accountId) : IGetApiRequest
{
    public string GetUrl => $"vacancies/{accountId}";
}