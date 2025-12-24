using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.Vacancies;

namespace SFA.DAS.EmployerAccounts.UnitTests.Infrastructure.OuterApi.Requests.Vacancies;

public class WhenBuildingGetVacanciesApiRequest
{
    [Test, AutoData]
    public void Then_The_Url_Is_Correctly_Constructed(long accountId)
    {
        var actual = new GetVacanciesApiRequest(accountId);

        actual.GetUrl.Should().Be($"vacancies/{accountId}");
    }
}