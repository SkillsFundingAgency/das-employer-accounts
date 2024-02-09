using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.SearchOrganisation;
using SFA.DAS.ReferenceData.Types.DTO;

namespace SFA.DAS.EmployerAccounts.UnitTests.Infrastructure.OuterApi.Requests.SearchOrganisation
{
    public class WhenBuildingGetLatestDetailsRequest
    {
        [Test, AutoData]
        public void Then_The_Request_Is_Correctly_Build(OrganisationType organisationType, string identifier)
        {
            var actual = new GetLatestDetailsRequest(organisationType, identifier);

            var expected = $"SearchOrganisation/review?identifier={identifier}&organisationType={organisationType}";

            actual.GetUrl.Should().Be(expected);
        }
    }
}