using AutoMapper;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.Types.Dtos;
using SFA.DAS.EmployerAccounts.Models.CommitmentsV2;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Services;

public class CommitmentsV2Service(
    ICommitmentsV2ApiClient commitmentsApiClient,
    IMapper mapper,
    IEncodingService encodingService)
    : ICommitmentV2Service
{
    public async Task<IEnumerable<Apprenticeship>> GetDraftApprenticeships(Cohort cohort)
    {
        var draftApprenticeshipsResponse = await commitmentsApiClient.GetDraftApprenticeships(cohort.Id);            
        return mapper.Map<IEnumerable<DraftApprenticeshipDto>, IEnumerable<Apprenticeship>>(draftApprenticeshipsResponse.DraftApprenticeships,
            opt =>
            {
                opt.AfterMap((src, dest) =>
                {
                    dest.ToList().ForEach(c =>
                    {
                        c.SetHashId(encodingService);
                        c.SetCohort(cohort);
                        c.SetTrainingProvider(cohort.TrainingProvider.First());
                    });
                });
            });
    }

    public async Task<IEnumerable<Cohort>> GetCohorts(long? accountId)
    {
        var cohortSummary = await commitmentsApiClient.GetCohorts(new GetCohortsRequest { AccountId = accountId });
        var trainingProvider = mapper.Map<IEnumerable<CohortSummary>, IEnumerable<TrainingProvider>>(cohortSummary.Cohorts);

        return mapper.Map<IEnumerable<CohortSummary>, IEnumerable<Cohort>>(cohortSummary.Cohorts,
            opt =>
            {   
                opt.AfterMap((src, dest) =>
                {
                    dest.ToList().ForEach(c =>
                    {
                        c.SetHashId(encodingService);
                        c.SetTrainingProvider(trainingProvider);
                    });                       
                });
            });
    }        

    public async Task<IEnumerable<Apprenticeship>> GetApprenticeships(long accountId)
    {
        var apprenticeship = await commitmentsApiClient.GetApprenticeships(new GetApprenticeshipsRequest { AccountId = accountId });
        var trainingProvider = mapper.Map<IEnumerable<GetApprenticeshipsResponse.ApprenticeshipDetailsResponse>, IEnumerable<TrainingProvider>>(apprenticeship.Apprenticeships);

        return mapper.Map<IEnumerable<GetApprenticeshipsResponse.ApprenticeshipDetailsResponse>, IEnumerable<Apprenticeship>>(apprenticeship.Apprenticeships,
            opt =>
            {
                opt.AfterMap((src, dest) =>
                {
                    dest.ToList().ForEach(c =>
                    {
                        c.SetHashId(encodingService);
                        c.SetTrainingProvider(trainingProvider.First());
                    });
                });
            });
    }

    public async Task<List<Cohort>> GetEmployerCommitments(long employerAccountId)
    {            
        var request = new GetCohortsRequest { AccountId = employerAccountId };
        var commitmentItems = await commitmentsApiClient.GetCohorts(request);

        if (commitmentItems == null || !commitmentItems.Cohorts.Any())
        {
            return [];
        }

        return commitmentItems.Cohorts.Where(x => x.CommitmentStatus != CommitmentStatus.Deleted)
            .Select(x => new Cohort { Id = x.CohortId }).ToList();
    }
}