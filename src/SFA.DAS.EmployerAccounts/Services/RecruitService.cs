using AutoMapper;
using SFA.DAS.EmployerAccounts.Models.Recruit;
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

    public async Task<Vacancy> GetVacancies(long accountId)
    {
        var request = new GetVacanciesApiRequest(accountId);

        var response = await _outerApiClient.Get<GetVacanciesApiResponse>(request);

        var vacancy = response.Vacancies.FirstOrDefault();
        return vacancy == null ? null : _mapper.Map<VacancySummary, Vacancy>(vacancy);
    }
}