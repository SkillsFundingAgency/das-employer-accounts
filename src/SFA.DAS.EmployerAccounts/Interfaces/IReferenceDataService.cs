using SFA.DAS.EmployerAccounts.Models.ReferenceData;
using SFA.DAS.ReferenceData.Types.DTO;
using CommonOrganisationType = SFA.DAS.Common.Domain.Types.OrganisationType;

namespace SFA.DAS.EmployerAccounts.Interfaces;

public interface IReferenceDataService
{   
    Task<PagedResponse<OrganisationName>> SearchOrganisations(
        string searchTerm, 
        int pageNumber = 1, 
        int pageSize = 20,
        CommonOrganisationType? organisationType = null);

    Task<Organisation> GetLatestDetails(CommonOrganisationType organisationType, string identifier);

    /// <summary>
    ///     Returns true if the supplied organisation type can be retrieved by id.
    /// </summary>
    /// <param name="organisationType"></param>
    /// <returns>
    /// True if the supplied organisation supports fetching by ID otherwise false.
    /// </returns>
    /// <remarks>
    ///     Companies House and the Charity commission 
    /// </remarks>
    Task<bool> IsIdentifiableOrganisationType(CommonOrganisationType organisationType);
}