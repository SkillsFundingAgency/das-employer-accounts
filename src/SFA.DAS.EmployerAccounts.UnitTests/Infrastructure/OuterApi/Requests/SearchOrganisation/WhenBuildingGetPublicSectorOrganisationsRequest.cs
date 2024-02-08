using System.Web;
using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.SearchOrganisation;

namespace SFA.DAS.EmployerAccounts.UnitTests.Infrastructure.OuterApi.Requests.SearchOrganisation
{
    public class WhenBuildingGetPublicSectorOrganisationsRequest
    {
        [Test, AutoData]
        public void Then_The_Request_Is_Correctly_Build(string searchTerm, int pageNumber, int pageSize)
        {
            var actual = new GetPublicSectorOrganisationsRequest(searchTerm, pageNumber, pageSize);

            var expected = $"searchOrganisation/publicsectorbodies?searchTerm={HttpUtility.UrlEncode(searchTerm)}&pageNumber={pageNumber}&pageSize={pageSize}";

            actual.GetUrl.Should().Be(expected);
        }
    }
}