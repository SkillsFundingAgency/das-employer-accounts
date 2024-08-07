﻿using AutoMapper;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.Types.Dtos;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Models.CommitmentsV2;
using SFA.DAS.EmployerAccounts.Services;
using SFA.DAS.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerAccounts.UnitTests.Services.AccountCohorts
{
    public class WhenIGetAccountCohorts
    {
        private Mock<ICommitmentsV2ApiClient> _mockCommitmentsV2ApiClient;
        private Mock<IEncodingService> _mockEncodingService;
        private Mock<IMapper> _mockMapper;
        private long _accountId = 123;
        private CommitmentsV2Service _sut;       

        [SetUp]
        public void Arrange()
        {
            _mockMapper = new Mock<IMapper>();
            _mockCommitmentsV2ApiClient = new Mock<ICommitmentsV2ApiClient>();
            _mockEncodingService = new Mock<IEncodingService>();
            _sut = new CommitmentsV2Service(_mockCommitmentsV2ApiClient.Object, _mockMapper.Object, _mockEncodingService.Object);
        }

        [Test]
        public async Task ThenGetApprenticeshipsResponse()
        {
            //Arrange
            _mockCommitmentsV2ApiClient
                .Setup(c => c.GetApprenticeships(It.Is<GetApprenticeshipsRequest>(r => r.AccountId == _accountId)))
                .Returns(Task.FromResult(CreateApprenticeshipResponse()));
            var apprenticeships = new List<Apprenticeship>  { new Apprenticeship { ApprenticeshipStatus = EmployerAccounts.Models.CommitmentsV2.ApprenticeshipStatus.Approved,  FirstName ="FirstName" , LastName = "LastName" } };
            _mockMapper
             .Setup(m => m.Map<IEnumerable<GetApprenticeshipsResponse.ApprenticeshipDetailsResponse>, IEnumerable<Apprenticeship>>
             (It.IsAny<IEnumerable<GetApprenticeshipsResponse.ApprenticeshipDetailsResponse>>(),
             It.IsAny<Action<IMappingOperationOptions<IEnumerable<GetApprenticeshipsResponse.ApprenticeshipDetailsResponse>, IEnumerable<Apprenticeship>>>>()))
             .Returns(apprenticeships);

            //Act
            var result =await _sut.GetApprenticeships(_accountId);

            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count().Equals(1), Is.True);
        }

        [Test]
        public async Task ThenGetCohortsResponse()
        {
            //Arrange
            _mockCommitmentsV2ApiClient.Setup(c => c.GetCohorts(It.Is<GetCohortsRequest>(r => r.AccountId == _accountId)))
               .Returns(Task.FromResult(GetCohortsResponseForWithTrainingProviderStaus()));
            _mockEncodingService.Setup(x => x.Encode(It.IsAny<long>(), EncodingType.CohortReference)).Returns((long y, EncodingType z) => y + "_Encoded");

            var cohorts = new List<Cohort>()
            {
                new Cohort { Id = 1 }
            };

            _mockMapper.Setup(m => m.Map<IEnumerable<CohortSummary>, IEnumerable<Cohort>>(It.IsAny<CohortSummary[]>(), 
                It.IsAny<Action<IMappingOperationOptions<IEnumerable<CohortSummary>, IEnumerable<Cohort>>>>()))
            .Returns(cohorts);

            //Act
            var result = await _sut.GetCohorts(_accountId);

            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count().Equals(1), Is.True);
        }

        [Test]
        public async Task ThenGetDraftApprenticeshipsResponse()
        {
            //Arrange
            _mockCommitmentsV2ApiClient.Setup(c => c.GetDraftApprenticeships(123)).Returns(Task.FromResult(GetDraftApprenticeshipsResponse()));
            var apprenticeships = new List<Apprenticeship> { new Apprenticeship { FirstName = "FirstName", LastName = "LastName" } };
            _mockMapper.Setup(m => m.Map<IEnumerable<DraftApprenticeshipDto>, IEnumerable<Apprenticeship>>(It.IsAny<IReadOnlyCollection<DraftApprenticeshipDto>>(),
               It.IsAny<Action<IMappingOperationOptions<IEnumerable<DraftApprenticeshipDto>, IEnumerable<Apprenticeship>>>>()))
           .Returns(apprenticeships);


            //Act
            var result = await _sut.GetDraftApprenticeships(new Cohort {Id = 123});

            //Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count().Equals(1), Is.True);
        }


        private GetApprenticeshipsResponse CreateApprenticeshipResponse()
        {
            IEnumerable<GetApprenticeshipsResponse.ApprenticeshipDetailsResponse> apprenticeships = new List<GetApprenticeshipsResponse.ApprenticeshipDetailsResponse>()
            {
                new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse
                {
                    Id = 1,
                    FirstName = "FirstName",
                    LastName = "LastName",
                    Uln = "Uln",
                    EmployerName = "EmployerName",
                    CourseName = "CourseName",
                    StartDate = new DateTime(2020, 5, 1),
                    EndDate = new DateTime(2022, 5, 1),
                    ApprenticeshipStatus = SFA.DAS.CommitmentsV2.Types.ApprenticeshipStatus.Live,
                    ProviderName = "ProviderName"
                }
            };

            return new GetApprenticeshipsResponse() { Apprenticeships = apprenticeships, TotalApprenticeships = 1, TotalApprenticeshipsFound = 1, TotalApprenticeshipsWithAlertsFound = 0 };
        }


        private GetCohortsResponse GetCohortsResponseForWithTrainingProviderStaus()
        {
            IEnumerable<CohortSummary> cohorts = new List<CohortSummary>()
            {
                new CohortSummary
                {
                    CohortId = 4,
                    AccountId = 1,
                    ProviderId = 4,
                    ProviderName = "Provider4",
                    NumberOfDraftApprentices = 1,
                    IsDraft = false,
                    WithParty = Party.Provider,
                    CreatedOn = DateTime.Now
                },
            };

            return new GetCohortsResponse(cohorts);
        }

        private GetDraftApprenticeshipsResponse GetDraftApprenticeshipsResponse()
        {
            IReadOnlyCollection<DraftApprenticeshipDto> draftApprenticeships = new List<DraftApprenticeshipDto>()
            {
                new DraftApprenticeshipDto
                {
                    Id = 1,
                    FirstName = "FirstName",
                    LastName = "LastName",
                    DateOfBirth = new DateTime(2000, 1 ,1 ),
                    Cost = 100,
                    StartDate = new DateTime(2020, 5, 1),
                    EndDate = new DateTime(2022, 5, 1),
                    CourseCode = "CourseCode",
                    CourseName = "CourseName"
                }
            };

            var draftApprenticeshipsResponse = new GetDraftApprenticeshipsResponse() { DraftApprenticeships = draftApprenticeships };
            return draftApprenticeshipsResponse;
        }
    }
}
