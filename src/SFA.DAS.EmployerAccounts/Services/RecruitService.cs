using AutoMapper;
using Newtonsoft.Json;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Models.Recruit;
using SFA.DAS.EmployerAccounts.Dtos;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.Vacancies;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Responses.Vacancies;
using SFA.DAS.EmployerAccounts.Interfaces.OuterApi;

namespace SFA.DAS.EmployerAccounts.Services;

public class RecruitService : IRecruitService
{
    private readonly IOuterApiClient _outerApiClient;
    private readonly IMapper _mapper;

    public RecruitService(
        IOuterApiClient outerApiClient,
        IMapper mapper)
    {
        _outerApiClient = outerApiClient;
        _mapper = mapper;
    }

    public async Task<IEnumerable<Vacancy>> GetVacancies(long accountId)
    {
        var request = new GetVacanciesApiRequest(accountId);

        var response = await _outerApiClient.Get<GetVacanciesApiResponse>(request);
        
        return _mapper.Map<IEnumerable<VacancySummary>, IEnumerable<Vacancy>>(response.Vacancies);
    }
}