using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.SearchOrganisation;

namespace SFA.DAS.EmployerAccounts.UnitTests.Infrastructure.OuterApi.Requests.SearchOrganisation
{
    public class WhenBuildingGetCharityRequest
    {
        [Test, AutoData]
        public void Then_The_Request_Is_Correctly_Build(int registrationNumber)
        {
            var actual = new GetCharityRequest(registrationNumber);

            var expected = $"SearchOrganisation/charities/{registrationNumber}";

            actual.GetUrl.Should().Be(expected);
        }
    }
}
