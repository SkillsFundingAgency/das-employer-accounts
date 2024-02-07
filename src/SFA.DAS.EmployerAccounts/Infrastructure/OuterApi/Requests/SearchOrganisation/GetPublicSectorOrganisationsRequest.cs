using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspose.Pdf;
using System.Web;
using SFA.DAS.EmployerAccounts.Interfaces.OuterApi;

namespace SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.SearchOrganisation
{
    public class GetPublicSectorOrganisationsRequest : IGetApiRequest
    {
        private readonly string SearchTerm;
        private readonly int PageNumber;
        private readonly int PageSize;


        public GetPublicSectorOrganisationsRequest(string searchTerm, int pageNumber, int pageSize)
        {
            SearchTerm = searchTerm;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public string GetUrl => $"searchOrganisation/publicsectorbodies?searchTerm={HttpUtility.UrlEncode(SearchTerm)}&pageNumber={PageNumber}&pageSize={PageSize}";

    }
}