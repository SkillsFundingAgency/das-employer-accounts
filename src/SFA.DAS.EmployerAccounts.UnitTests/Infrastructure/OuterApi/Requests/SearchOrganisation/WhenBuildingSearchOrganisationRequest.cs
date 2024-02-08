using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.SearchOrganisation;

namespace SFA.DAS.EmployerAccounts.UnitTests.Infrastructure.OuterApi.Requests.SearchOrganisation
{
    public class WhenBuildingSearchOrganisationRequest
    {
        [Test, AutoData]
        public void Then_The_Request_Is_Correctly_Build(string searchTerm, int maximumResults)
        {
            var actual = new SearchOrganisationRequest(searchTerm, maximumResults);

            var expected = $"SearchOrganisation/?searchTerm={searchTerm}&maximumResults={maximumResults}";

            actual.GetUrl.Should().Be(expected);
        }
    }
}