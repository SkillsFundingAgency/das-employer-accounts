using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.Vacancies;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Responses.Vacancies;
using SFA.DAS.EmployerAccounts.Interfaces.OuterApi;
using SFA.DAS.EmployerAccounts.Models.Recruit;
using SFA.DAS.EmployerAccounts.Services;

namespace SFA.DAS.EmployerAccounts.UnitTests.Services.Recruit
{
    public class GetVacancies
    {   
        private Mock<IMapper> _mockMapper;

        private long _accountId;
        private string _hashedAccountId;
        private List<VacancySummary> _vacancies;

        private RecruitService _sut;
        private Mock<IOuterApiClient> _outerApiClient;

        [SetUp]
        public void Arrange()
        {
            _accountId = 1001;
            _hashedAccountId = "ABC123";
            _vacancies = [new VacancySummary { Title = "Test Vacancy" }];

            _mockMapper = new Mock<IMapper>();

            _outerApiClient = new Mock<IOuterApiClient>();
            _outerApiClient
                .Setup(x => x.Get<GetVacanciesApiResponse>(
                    It.Is<GetVacanciesApiRequest>(c => c.GetUrl.Contains(_accountId.ToString()))))
                .ReturnsAsync(new GetVacanciesApiResponse { Vacancies = _vacancies });

            _sut = new RecruitService(_outerApiClient.Object, _mockMapper.Object);
        }

        [Test]
        public async Task ThenTheMappedVacancyDataIsReturned()
        {
            //Arrange
            var testTitle = Guid.NewGuid().ToString();

            var vacancy =
                new Vacancy
                {
                    Title = testTitle,
                    NoOfSuccessfulApplications = 100,
                    NoOfNewApplications = 10,
                    NoOfUnsuccessfulApplications = 20
                };

            _mockMapper
                .Setup(m => m.Map<VacancySummary, Vacancy>(It.Is<VacancySummary>(c=>c.Title == "Test Vacancy")))
                .Returns(vacancy);

            //Act
            var result = await _sut.GetVacancies(_accountId);

            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Title, Is.EqualTo(testTitle));
            Assert.That(result.NoOfSuccessfulApplications, Is.EqualTo(100));
            Assert.That(result.NoOfNewApplications, Is.EqualTo(10));
            Assert.That(result.NoOfUnsuccessfulApplications, Is.EqualTo(20));
        }
        
        [Test]
        public async Task Then_If_No_Items_Null_Returned()
        {
            //Arrange
            _outerApiClient
                .Setup(x => x.Get<GetVacanciesApiResponse>(
                    It.Is<GetVacanciesApiRequest>(c => c.GetUrl.Contains(_accountId.ToString()))))
                .ReturnsAsync(new GetVacanciesApiResponse { Vacancies = [] });

            //Act
            var result = await _sut.GetVacancies(_accountId);

            //Assert
            Assert.That(result, Is.Null);
        }
    }
}
