using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Dtos;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.Vacancies;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Responses.Vacancies;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Interfaces.OuterApi;
using SFA.DAS.EmployerAccounts.Models.Recruit;
using SFA.DAS.EmployerAccounts.Services;

namespace SFA.DAS.EmployerAccounts.UnitTests.Services.Recruit
{
    public class GetVacancies
    {   
        private Mock<IMapper> _mockMapper;

        private long _accountId;
        private List<VacancySummary> _vacancies;

        private RecruitService _sut;

        [SetUp]
        public void Arrange()
        {
            _accountId = 1001;
            _vacancies = [new VacancySummary { Title = "Test Vacancy" }];

            _mockMapper = new Mock<IMapper>();

            var outerApiClient = new Mock<IOuterApiClient>();
            outerApiClient
                .Setup(x => x.Get<GetVacanciesApiResponse>(
                    It.Is<GetVacanciesApiRequest>(c => c.GetUrl.Contains(_accountId.ToString()))))
                .ReturnsAsync(new GetVacanciesApiResponse { Vacancies = _vacancies });

            _sut = new RecruitService(outerApiClient.Object, _mockMapper.Object);
        }

        [Test]
        public async Task ThenTheMappedVacancyDataIsReturned()
        {
            //Arrange
            var testTitle = Guid.NewGuid().ToString();

            var vacancies = new List<Vacancy>
            {
                new Vacancy
                {
                    Title = testTitle,
                    NoOfSuccessfulApplications = 100,
                    NoOfNewApplications = 10,
                    NoOfUnsuccessfulApplications = 20
                }
            };

            _mockMapper
                .Setup(m => m.Map<IEnumerable<VacancySummary>, 
                    IEnumerable<Vacancy>>(It.Is<List<VacancySummary>>(c=>c.First().Title == "Test Vacancy")))
                .Returns(vacancies);

            //Act
            var result = await _sut.GetVacancies(_accountId) as List<Vacancy>;

            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.Empty);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Title, Is.EqualTo(testTitle));
            Assert.That(result[0].NoOfSuccessfulApplications, Is.EqualTo(100));
            Assert.That(result[0].NoOfNewApplications, Is.EqualTo(10));
            Assert.That(result[0].NoOfUnsuccessfulApplications, Is.EqualTo(20));
        }
    }
}
