using SFA.DAS.EmployerAccounts.Interfaces.OuterApi;

namespace SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.SearchOrganisation
{
    public class GetIdentifiableOrganisationTypesRequest : IGetApiRequest
    {
        public string GetUrl => $"SearchOrganisation/IdentifiableOrganisationTypes";
    }
}