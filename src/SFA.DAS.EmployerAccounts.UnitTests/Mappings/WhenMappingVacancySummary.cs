using System;
using AutoMapper;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Responses.Vacancies;
using SFA.DAS.EmployerAccounts.Mappings;
using SFA.DAS.EmployerAccounts.Models.Recruit;

namespace SFA.DAS.EmployerAccounts.UnitTests.Mappings;

public class WhenMappingVacancySummary
{
    private IMapper _mapper;

    [SetUp]
    public void SetUp()
    {
        _mapper = new MapperConfiguration(c => c.AddProfile<VacancyMappings>()).CreateMapper();
    }

    [Test]
    public void Then_Archived_Status_Is_Mapped()
    {
        var source = new VacancySummary
        {
            Id = Guid.NewGuid(),
            Title = "Archived vacancy",
            Status = "Archived"
        };

        var actual = _mapper.Map<Vacancy>(source);

        actual.Status.Should().Be(VacancyStatus.Archived);
        actual.Title.Should().Be("Archived vacancy");
        actual.Id.Should().Be(source.Id);
    }
}
