using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.EmployerAccounts.Interfaces.OuterApi;

namespace SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.SearchOrganisation
{
    public class SearchOrganisationRequest : IGetApiRequest
    {
        private readonly string SearchTerm;
        private readonly int MaximumResults;

        public string GetUrl => $"SearchOrganisation/?searchTerm={SearchTerm}&maximumResults={MaximumResults}";

        public SearchOrganisationRequest(string searchTerm, int maximumResults = 500)
        {
            SearchTerm = searchTerm;
            MaximumResults = maximumResults;
        }
    }
}