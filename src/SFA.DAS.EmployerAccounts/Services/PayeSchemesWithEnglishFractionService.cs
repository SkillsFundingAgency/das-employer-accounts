using AutoMapper;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.Finance;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Responses.Finance;
using SFA.DAS.EmployerAccounts.Interfaces.OuterApi;
using SFA.DAS.EmployerAccounts.Models.PAYE;
using SFA.DAS.Encoding;
using DasEnglishFraction = SFA.DAS.EmployerAccounts.Models.Levy.DasEnglishFraction;

namespace SFA.DAS.EmployerAccounts.Services;

public class PayeSchemesWithEnglishFractionService : IPayeSchemesWithEnglishFractionService
{
    private readonly IOuterApiClient _outerApiClient;
    private readonly IPayeRepository _payeRepository;
    private readonly IEncodingService _encodingService;
    private readonly IMapper _mapper;

    public PayeSchemesWithEnglishFractionService(IOuterApiClient outerApiClient, IPayeRepository payeRepository, IEncodingService encodingService, IMapper mapper)
    {
        _outerApiClient = outerApiClient;
        _payeRepository = payeRepository;
        _encodingService = encodingService;
        _mapper = mapper;
    }
        
    public async Task<IEnumerable<PayeView>> GetPayeSchemes(long accountId)
    {
        var payeSchemes = await _payeRepository.GetPayeSchemesByAccountId(accountId);
        if (payeSchemes.Any())
        {
            await AddEnglishFractionToPayeSchemes(accountId, payeSchemes);
        }

        return payeSchemes;
    }

    private async Task AddEnglishFractionToPayeSchemes(long accountId, IList<PayeView> payeSchemes)
    {
        var hashedAccountId = _encodingService.Encode(accountId, EncodingType.AccountId);

        // TODO: Outer API should use long accountId
        var payeSchemeRefs = payeSchemes.Select(x => x.Ref).Where(x => !string.IsNullOrEmpty(x)).ToArray();

        // CON-5023 - Batching up API calls due to some employers having large amounts of PAYE schemes.
        var tasks = new List<Task<GetEnglishFractionCurrentResponse>>();
        
        const int batchSize = 50;
        
        var numberOfBatches = (int)Math.Ceiling((double)payeSchemeRefs.Length / batchSize);
        
        for (var batchIndex = 0; batchIndex < numberOfBatches; batchIndex++)
        {
            var currentPayeRefs = payeSchemeRefs.Skip(batchIndex * batchSize).Take(batchSize);
            var request = new GetEnglishFractionCurrentRequest(hashedAccountId, currentPayeRefs);
            tasks.Add(_outerApiClient.Get<GetEnglishFractionCurrentResponse>(request));
        }

        var responses = await Task.WhenAll(tasks);

        var englishFractions = _mapper.Map<List<DasEnglishFraction>>(responses.SelectMany(x => x.Fractions));
        
        foreach (var scheme in payeSchemes)
        {
            scheme.EnglishFraction = englishFractions.FirstOrDefault(x => x.EmpRef == scheme.Ref);
        }
    }
}