using System.Text.RegularExpressions;
using SFA.DAS.Caches;
using SFA.DAS.EmployerAccounts.Extensions;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.SearchOrganisation;
using SFA.DAS.EmployerAccounts.Interfaces.OuterApi;
using SFA.DAS.EmployerAccounts.Models.ReferenceData;
using SFA.DAS.ReferenceData.Types.DTO;
using Address = SFA.DAS.EmployerAccounts.Models.Organisation.Address;
using CommonOrganisationType = SFA.DAS.Common.Domain.Types.OrganisationType;
using OrganisationSubType = SFA.DAS.Common.Domain.Types.OrganisationSubType;
using ReferenceDataOrganisationType = SFA.DAS.ReferenceData.Types.DTO.OrganisationType;

namespace SFA.DAS.EmployerAccounts.Services;

public class ReferenceDataService : IReferenceDataService
{
    private readonly Lazy<Task<CommonOrganisationType[]>> _identifiableOrganisationTypes;
    private readonly IOuterApiClient _outerApiClient;
    private readonly IInProcessCache _inProcessCache;

    private readonly List<string> _termsToRemove = new List<string> { "ltd", "ltd.", "limited", "plc", "plc." };

    public ReferenceDataService(
        IInProcessCache inProcessCache,
        IOuterApiClient outerApiClient
        )
    {
        _inProcessCache = inProcessCache;
        _identifiableOrganisationTypes = new Lazy<Task<CommonOrganisationType[]>>(InitialiseOrganisationTypes);
        _outerApiClient = outerApiClient;
    }    

    public async Task<PagedResponse<OrganisationName>> SearchOrganisations(string searchTerm, int pageNumber = 1, int pageSize = 25, CommonOrganisationType? organisationType = null)
    {
        var result = await SearchOrganisations(searchTerm);

        if (result == null)
        {
            return new PagedResponse<OrganisationName>();
        }

        if (organisationType != null)
        {
            result = FilterOrganisationsByType(result, organisationType.Value);
        }

        return CreatePagedOrganisationResponse(pageNumber, pageSize, result);
    }

    public async Task<Organisation> GetLatestDetails(CommonOrganisationType organisationType, string identifier)
    {
        return await _outerApiClient.Get<Organisation>(new GetLatestDetailsRequest(organisationType.ToReferenceDataOrganisationType(), identifier));
    }

    public async Task<bool> IsIdentifiableOrganisationType(CommonOrganisationType organisationType)
    {
        if (organisationType.TryToReferenceDataOrganisationType(out ReferenceDataOrganisationType referenceDataType))
        {
            var locateableOrganisationTypes = await _identifiableOrganisationTypes.Value;

            return locateableOrganisationTypes.Contains(organisationType);
        }

        return false;
    }

    private async Task<CommonOrganisationType[]> InitialiseOrganisationTypes()
    {
        var result = await _outerApiClient.Get<ReferenceDataOrganisationType[]>(new GetIdentifiableOrganisationTypesRequest());

        var filteredOrganisationTypes = result
                    .Select(referenceDataOrganisationType => referenceDataOrganisationType.ToCommonOrganisationType())
                    .ToArray();

        return filteredOrganisationTypes;
    }

    private static List<OrganisationName> SortOrganisations(List<OrganisationName> result, string searchTerm)
    {
        var totalDocuments = result.Count;
        var averageFieldLength = result.Average(o => o.Name.Length);

        var scoredOrganisations = result
            .Select(o => new
            {
                Organisation = o,
                Score = CalculateBM25Score(o, searchTerm, totalDocuments, averageFieldLength)
            })
            .OrderByDescending(o => o.Score);

        return scoredOrganisations
            .Select(so => so.Organisation)
            .ToList();
    }
    
    private static double CalculateBM25Score(
        OrganisationName organisation,
        string searchTerm,
        int totalDocuments,
        double averageFieldLength,
        double k1 = 1.2,
        double b = 0.75)
    {
        var terms = searchTerm.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var name = organisation.Name.ToLower();

        double score = 0;
        foreach (var term in terms)
        {
            var termFrequency = name.Split(' ').Count(word => word.Equals(term));
            if (termFrequency == 0) continue;

            var docCount = totalDocuments; // In practice, you'd precompute how many documents contain the term.
            var idf = Math.Log((totalDocuments - docCount + 0.5) / (docCount + 0.5) + 1);

            var fieldLengthNormalization = 1 - b + b * (name.Length / averageFieldLength);
            score += idf * (termFrequency * (k1 + 1) / (termFrequency + k1 * fieldLengthNormalization));
        }

        return score;
    }

    private static List<OrganisationName> FilterOrganisationsByType(IEnumerable<OrganisationName> result, CommonOrganisationType organisationType)
    {
        if (organisationType == CommonOrganisationType.Other || organisationType == CommonOrganisationType.PublicBodies)
        {
            return result.Where(x => x.Type == CommonOrganisationType.Other || x.Type == CommonOrganisationType.PublicBodies).ToList();
        }
        return result.Where(x => x.Type == organisationType).ToList();
    }

    private async Task<List<OrganisationName>> SearchOrganisations(string searchTerm)
    {
        var cacheKey = $"SearchKey_{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(searchTerm))}";

        var result = _inProcessCache.Get<List<OrganisationName>>(cacheKey);
        if (result != null && result.Any()) return result;

        var orgs = await _outerApiClient.Get<IEnumerable<Organisation>>(new SearchOrganisationRequest(searchTerm));

        if (orgs == null) return [];

        var convertedOrgs = orgs
            .Select(ConvertToOrganisation)
            .Where(FilterInactiveOrgs)
            .ToList();

        result = SortOrganisations(convertedOrgs, searchTerm);

        _inProcessCache.Set(cacheKey, result, new TimeSpan(0, 15, 0));
        return result;
    }

    private bool FilterInactiveOrgs(OrganisationName organisation)
    {
        return organisation.OrganisationStatus == Models.Organisation.OrganisationStatus.None || organisation.OrganisationStatus == Models.Organisation.OrganisationStatus.Active;
    }

    private static PagedResponse<OrganisationName> CreatePagedOrganisationResponse(int pageNumber, int pageSize, List<OrganisationName> result)
    {
        return new PagedResponse<OrganisationName>
        {
            Data = result.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
            TotalPages = (int)Math.Ceiling(((decimal)result.Count / pageSize)),
            PageNumber = pageNumber,
            TotalResults = result.Count
        };
    }

    private OrganisationName ConvertToOrganisation(Organisation source)
    {
        return new OrganisationName
        {
            Address = new Address
            {
                Line1 = source.Address.Line1,
                Line2 = source.Address.Line2,
                Line3 = source.Address.Line3,
                Line4 = source.Address.Line4,
                Line5 = source.Address.Line5,
                Postcode = source.Address.Postcode
            },
            Name = source.Name,
            Code = source.Code,
            RegistrationDate = source.RegistrationDate,
            Sector = source.Sector,
            SubType = (OrganisationSubType)source.SubType,
            Type = source.Type.ToCommonOrganisationType(),
            OrganisationStatus = (Models.Organisation.OrganisationStatus)source.OrganisationStatus
        };
    }
}