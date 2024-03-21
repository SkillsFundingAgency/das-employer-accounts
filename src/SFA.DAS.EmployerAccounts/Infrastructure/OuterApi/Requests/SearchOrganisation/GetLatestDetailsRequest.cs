using SFA.DAS.EmployerAccounts.Interfaces.OuterApi;
using SFA.DAS.ReferenceData.Types.DTO;

namespace SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.SearchOrganisation
{
    public class GetLatestDetailsRequest : IGetApiRequest
    {
        private readonly string Identifier;
        public readonly OrganisationType OrganisationType;

        public string GetUrl => $"SearchOrganisation/review?identifier={Identifier}&organisationType={OrganisationType}";

        public GetLatestDetailsRequest(OrganisationType organisationType, string identifier)
        {
            OrganisationType = organisationType;
            Identifier = identifier;
        }
    }
}