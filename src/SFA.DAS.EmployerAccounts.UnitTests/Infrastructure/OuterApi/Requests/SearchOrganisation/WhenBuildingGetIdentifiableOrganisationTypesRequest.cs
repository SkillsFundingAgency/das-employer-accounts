using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.SearchOrganisation;

namespace SFA.DAS.EmployerAccounts.UnitTests.Infrastructure.OuterApi.Requests.SearchOrganisation
{
    public class WhenBuildingGetIdentifiableOrganisationTypesRequest
    {
        [Test, AutoData]
        public void Then_The_Request_Is_Correctly_Build()
        {
            var actual = new GetIdentifiableOrganisationTypesRequest();

            var expected = $"SearchOrganisation/IdentifiableOrganisationTypes";

            actual.GetUrl.Should().Be(expected);
        }
    }
}