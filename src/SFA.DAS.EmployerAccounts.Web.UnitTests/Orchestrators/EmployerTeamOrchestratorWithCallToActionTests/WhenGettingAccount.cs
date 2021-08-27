﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Authorization.Services;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Models.Recruit;
using SFA.DAS.EmployerAccounts.Models.CommitmentsV2;
using SFA.DAS.EmployerAccounts.Models.Reservations;
using SFA.DAS.EmployerAccounts.Queries.GetSingleCohort;
using SFA.DAS.EmployerAccounts.Queries.GetReservations;
using SFA.DAS.EmployerAccounts.Queries.GetVacancies;
using SFA.DAS.EmployerAccounts.Web.Orchestrators;
using SFA.DAS.EmployerAccounts.Web.ViewModels;
using SFA.DAS.EmployerAccounts.Queries.GetApprenticeship;
using SFA.DAS.EmployerAccounts.Web.Models;
using System.Net;
using FluentAssertions;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.NLog.Logger;
using System.Net.Http;
using SFA.DAS.EmployerAccounts.Configuration;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Orchestrators.EmployerTeamOrchestratorWithCallToActionTests
{
    public class WhenGettingAccount
    {
        private string HashedAccountId = Guid.NewGuid().ToString();
        private AccountDashboardViewModel AccountDashboardViewModel;

        private const long AccountId = 123;
        private readonly string UserId = Guid.NewGuid().ToString();

        private EmployerTeamOrchestratorWithCallToAction _sut;
        private Mock<IMediator> _mediator;
        private Mock<EmployerTeamOrchestrator> _employerTeamOrchestrator;
        
        private Mock<ICurrentDateTime> _mockCurrentDateTime;
        private Mock<IAccountApiClient> _mockAccountApiClient;
        private Mock<IMapper> _mockMapper;
        private Mock<ICookieStorageService<AccountContext>> _mockAccountContext;
        private Mock<ILog> _mockLogger;

        [SetUp]
        public void Arrange()
        {
            _mockAccountContext = new Mock<ICookieStorageService<AccountContext>>();

            _employerTeamOrchestrator = new Mock<EmployerTeamOrchestrator>();

            AccountDashboardViewModel = new AccountDashboardViewModel
            {
                HashedAccountId = HashedAccountId
            };

            _employerTeamOrchestrator
                .Setup(m => m.GetAccount(HashedAccountId, UserId))
                .ReturnsAsync(new OrchestratorResponse<AccountDashboardViewModel> {
                    Data = AccountDashboardViewModel,
                    Status = HttpStatusCode.OK });

            _mediator = new Mock<IMediator>();

            _mediator.Setup(x => x.SendAsync(It.IsAny<GetVacanciesRequest>()))
                .ReturnsAsync(new GetVacanciesResponse
                {
                     Vacancies = new List<Vacancy>()
                });

            _mediator.Setup(m => m.SendAsync(It.Is<GetReservationsRequest>(q => q.HashedAccountId == HashedAccountId)))
                   .ReturnsAsync(new GetReservationsResponse
                   {
                       Reservations = new List<Reservation>
                       {
                             new Reservation
                             {
                                 AccountId = 123
                             }
                       }
                   });

            _mediator.Setup(m => m.SendAsync(It.Is<GetApprenticeshipsRequest>(q => q.HashedAccountId == HashedAccountId)))
                 .ReturnsAsync(new GetApprenticeshipsResponse
                 {
                    Apprenticeships = new List<Apprenticeship>
                    {
                        new Apprenticeship { FirstName = "FirstName" }
                    }
                 });

            var Cohort = new Cohort()
            {
                Id = 1,              
                CohortStatus = EmployerAccounts.Models.CommitmentsV2.CohortStatus.WithTrainingProvider,
                NumberOfDraftApprentices = 1,
                Apprenticeships = new List<Apprenticeship> 
                        {
                            new Apprenticeship()
                            {
                                Id = 2,
                                FirstName = "FirstName",
                                LastName = "LastName",
                                CourseStartDate = new DateTime(2020,5,1),
                                CourseEndDate = new DateTime(2022,1,1),
                                CourseName = "CourseName"                               
                            }
                        }
            };
            _mediator.Setup(x => x.SendAsync(It.IsAny<GetSingleCohortRequest>()))
                    .ReturnsAsync(new GetSingleCohortResponse 
                    {  
                        Cohort = Cohort

                    });

            _mockCurrentDateTime = new Mock<ICurrentDateTime>();

            _mockAccountApiClient = new Mock<IAccountApiClient>();

            _mockAccountApiClient.Setup(c => c.GetAccount(HashedAccountId)).ReturnsAsync(new AccountDetailViewModel
                {ApprenticeshipEmployerType = "Levy"});
           
            _mockMapper = new Mock<IMapper>();

            _mockLogger = new Mock<ILog>();

            _sut = new EmployerTeamOrchestratorWithCallToAction(
                _employerTeamOrchestrator.Object,
                _mediator.Object, 
                _mockCurrentDateTime.Object, 
                _mockAccountApiClient.Object, 
                _mockMapper.Object, 
                _mockAccountContext.Object,
                _mockLogger.Object,
                Mock.Of<EmployerAccountsConfiguration>());
        }

        [Test]
        public async Task ThenAccountDataIsRetrievedFromTheTeamOrchestrator()
        {
            // Act
            await _sut.GetAccount(HashedAccountId, UserId);

            //Assert
            _employerTeamOrchestrator.Verify(m => m.GetAccount(HashedAccountId, UserId), Times.Once);
        }

        [Test]
        public async Task ThenExpectedAccountDataIsReturnedFromTheTeamOrchestrator()
        {
            // Act
            var result = await _sut.GetAccount(HashedAccountId, UserId);

            //Assert
            result.Data.Should().Be(AccountDashboardViewModel);
        }

        [Test]
        public async Task ThenCallToActionDataIsRetrievedWhenAccountContextCookieIsNotSet()
        {
            // Act
            var result = await _sut.GetAccount(HashedAccountId, UserId);

            //Assert
            result.Data.CallToActionViewModel.Should().NotBeNull();
        }

        [Test]
        public async Task ThenCallToActionDataIsRetrievedWhenAccountContextIsNonLevy()
        {
            // arrange
            _mockAccountContext
                .Setup(m => m.Get(EmployerTeamOrchestratorWithCallToAction.AccountContextCookieName))
                .Returns(new AccountContext { HashedAccountId = HashedAccountId, ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy });

            // Act
            var result = await _sut.GetAccount(HashedAccountId, UserId);

            //Assert
            result.Data.CallToActionViewModel.Should().NotBeNull();
        }

        [Test]
        public async Task ThenCallToActionDataIsNotRetrievedWhenAccountContextIsLevy()
        {
            // arrange
            _mockAccountContext
                .Setup(m => m.Get(EmployerTeamOrchestratorWithCallToAction.AccountContextCookieName))
                .Returns(new AccountContext { HashedAccountId = HashedAccountId, ApprenticeshipEmployerType = ApprenticeshipEmployerType.Levy });

            // Act
            var result = await _sut.GetAccount(HashedAccountId, UserId);

            //Assert
            result.Data.CallToActionViewModel.Should().BeNull();
        }

        [Test]
        public async Task ThenTheAccountContextIsSavedWhenAccountContextIsLevy()
        {
            // arrange
            AccountDashboardViewModel.ApprenticeshipEmployerType = ApprenticeshipEmployerType.Levy;
            
            // Act
            var result = await _sut.GetAccount(HashedAccountId, UserId);

            //Assert
            _mockAccountContext.Verify(m => m.Delete(EmployerTeamOrchestratorWithCallToAction.AccountContextCookieName), Times.Once);
            _mockAccountContext.Verify(m => m.Create(It.Is<AccountContext>(a => 
                (a.HashedAccountId == HashedAccountId) && (a.ApprenticeshipEmployerType == ApprenticeshipEmployerType.Levy)),
                EmployerTeamOrchestratorWithCallToAction.AccountContextCookieName,
                It.IsAny<int>()),
                Times.Once);
        }

        [Test]
        public async Task ThenTheAccountContextIsSavedWhenAccountContextIsNonLevy()
        {
            // arrange
            AccountDashboardViewModel.ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy;

            // Act
            var result = await _sut.GetAccount(HashedAccountId, UserId);

            //Assert
            _mockAccountContext.Verify(m => m.Delete(EmployerTeamOrchestratorWithCallToAction.AccountContextCookieName), Times.Once);
            _mockAccountContext.Verify(m => m.Create(It.Is<AccountContext>(a =>
                (a.HashedAccountId == HashedAccountId) && (a.ApprenticeshipEmployerType == ApprenticeshipEmployerType.NonLevy)),
                EmployerTeamOrchestratorWithCallToAction.AccountContextCookieName,
                It.IsAny<int>()),
                Times.Once);
        }
               
        [Test]
        public async Task ThenCallToActionDataIsRetrievedWhenAccountContextAccountHasChanged()
        {
            // arrange
            _mockAccountContext
                .Setup(m => m.Get(EmployerTeamOrchestratorWithCallToAction.AccountContextCookieName))
                .Returns(new AccountContext { HashedAccountId = Guid.NewGuid().ToString() , ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy });

            // Act
            var result = await _sut.GetAccount(HashedAccountId, UserId);

            //Assert
            result.Data.CallToActionViewModel.Should().NotBeNull();
        }

        [Test]
        public async Task ThenCallToActionDataIsNotRetrievedWhenVacancyServiceHasFailed()
        {
            //Arrange

            _mediator.Setup(x => x.SendAsync(It.IsAny<GetVacanciesRequest>()))
               .ReturnsAsync(new GetVacanciesResponse
               {
                   HasFailed = true
               });

            // Act
            var result = await _sut.GetAccount(HashedAccountId, UserId);

            //Assert
            result.Data.CallToActionViewModel.Should().BeNull();
        }

        [Test]
        public async Task ThenCallToActionDataIsNotRetrievedWhenVacancyServiceHasException()
        {
            //Arrange

            _mediator.Setup(x => x.SendAsync(It.IsAny<GetVacanciesRequest>()))
               .ThrowsAsync(new Exception());

            // Act
            var result = await _sut.GetAccount(HashedAccountId, UserId);

            //Assert
            result.Data.CallToActionViewModel.Should().BeNull();
        }

        [Test]
        public async Task ThenErrorIsLoggedWhenVacancyServiceHasException()
        {
            //Arrange
            var exception = new Exception();
            _mediator.Setup(x => x.SendAsync(It.IsAny<GetVacanciesRequest>()))
               .ThrowsAsync(exception);

            // Act
            var result = await _sut.GetAccount(HashedAccountId, UserId);

            //Assert
            _mockLogger.Verify(m => m.Error(exception, $"An error occured whilst trying to retrieve account CallToAction: {HashedAccountId}"), Times.Once);
        }

        [Test]
        public async Task ThenShouldReturnTheVacancies()
        {
            //Arrange            
            var vacancy = new Vacancy { Title = Guid.NewGuid().ToString() };
            var vacancies = new List<Vacancy> { vacancy };

            var expectedtitle = Guid.NewGuid().ToString();
            var expectedvacancy = new VacancyViewModel { Title = expectedtitle };
            var expectedVacancies = new List<VacancyViewModel> { expectedvacancy };

            _mediator.Setup(x => x.SendAsync(It.IsAny<GetVacanciesRequest>()))
               .ReturnsAsync(new GetVacanciesResponse
               {
                   Vacancies = vacancies
               });

            _mockMapper.Setup(m => m.Map<IEnumerable<Vacancy>, IEnumerable<VacancyViewModel>>(vacancies))
                .Returns(expectedVacancies);

            // Act
            var actual = await _sut.GetAccount(HashedAccountId, UserId);

            //Assert
            Assert.IsNotNull(actual.Data);
            Assert.AreEqual(1, actual.Data.CallToActionViewModel.VacanciesViewModel.VacancyCount);
            Assert.AreEqual(expectedvacancy.Title, actual.Data.CallToActionViewModel.VacanciesViewModel.Vacancies.First().Title);
            _mockMapper.Verify(m => m.Map<IEnumerable<Vacancy>, IEnumerable<VacancyViewModel>>(vacancies), Times.Once);
        }

        [Test]
        public async Task ThenCallToActionDataIsNotRetrievedWhenReservationsServiceHasFailed()
        {
            //Arrange

            _mediator.Setup(x => x.SendAsync(It.IsAny<GetReservationsRequest>()))
               .ReturnsAsync(new GetReservationsResponse
               {
                   HasFailed = true
               });

            // Act
            var result = await _sut.GetAccount(HashedAccountId, UserId);

            //Assert
            result.Data.CallToActionViewModel.Should().BeNull();
        }
        
        [Test]
        public async Task ThenCallToActionDataIsNotRetrievedWhenReservationsServiceHasException()
        {
            //Arrange

            _mediator.Setup(x => x.SendAsync(It.IsAny<GetReservationsRequest>()))
               .ThrowsAsync(new Exception());

            // Act
            var result = await _sut.GetAccount(HashedAccountId, UserId);

            //Assert
            result.Data.CallToActionViewModel.Should().BeNull();
        }

        [Test]
        public async Task ThenErrorIsLoggedWhenReservationsServiceHasException()
        {
            //Arrange
            var exception = new Exception();
            _mediator.Setup(x => x.SendAsync(It.IsAny<GetReservationsRequest>()))
               .ThrowsAsync(exception);

            // Act
            var result = await _sut.GetAccount(HashedAccountId, UserId);

            //Assert
            _mockLogger.Verify(m => m.Error(exception, $"An error occured whilst trying to retrieve account CallToAction: {HashedAccountId}"), Times.Once);
        }

        [Test]
        public async Task ThenShouldGetReservationsCount()
        {
            // Act
            var result = await _sut.GetAccount(HashedAccountId, UserId);

            //Assert
            Assert.AreEqual(1, result.Data.CallToActionViewModel.ReservationsCount);
        }

        [Test]
        public async Task ThenCallToActionDataIsNotRetrievedWhenApprenticeshipsServiceHasFailed()
        {
            //Arrange

            _mediator.Setup(x => x.SendAsync(It.IsAny<GetApprenticeshipsRequest>()))
               .ReturnsAsync(new GetApprenticeshipsResponse
               {
                   HasFailed = true
               });

            // Act
            var result = await _sut.GetAccount(HashedAccountId, UserId);

            //Assert
            result.Data.CallToActionViewModel.Should().BeNull();
        }

        [Test]
        public async Task ThenCallToActionDataIsNotRetrievedWhenApprenticeshipsServiceHasException()
        {
            //Arrange

            _mediator.Setup(x => x.SendAsync(It.IsAny<GetApprenticeshipsRequest>()))
               .ThrowsAsync(new Exception());

            // Act
            var result = await _sut.GetAccount(HashedAccountId, UserId);

            //Assert
            result.Data.CallToActionViewModel.Should().BeNull();
        }

        [Test]
        public async Task ThenErrorIsLoggedWhenApprenticeshipsServiceHasException()
        {
            //Arrange
            var exception = new Exception();
            _mediator.Setup(x => x.SendAsync(It.IsAny<GetApprenticeshipsRequest>()))
               .ThrowsAsync(exception);

            // Act
            var result = await _sut.GetAccount(HashedAccountId, UserId);

            //Assert
            _mockLogger.Verify(m => m.Error(exception, $"An error occured whilst trying to retrieve account CallToAction: {HashedAccountId}"), Times.Once);
        }

        [Test]
        public async Task ThenShouldGetApprenticeshipResponse()
        {
            //Arrange
            var apprenticeships = new List<Apprenticeship> { new Apprenticeship { FirstName = "FirstName" } };
            _mediator.Setup(m => m.SendAsync(It.Is<GetApprenticeshipsRequest>(q => q.HashedAccountId == HashedAccountId)))
                .ReturnsAsync(new GetApprenticeshipsResponse { Apprenticeships = apprenticeships });
            var expectedApprenticeship = new List<ApprenticeshipViewModel>() { new ApprenticeshipViewModel { ApprenticeshipFullName = "FullName" } };
            _mockMapper.Setup(m => m.Map<IEnumerable<Apprenticeship>, IEnumerable<ApprenticeshipViewModel>>(apprenticeships)).Returns(expectedApprenticeship);

            //Act
            var result = await _sut.GetAccount(HashedAccountId, UserId);

            //Assert
            Assert.IsTrue(result.Data.CallToActionViewModel.Apprenticeships.Count().Equals(1));
        }

        [Test]
        public async Task ThenCallToActionDataIsNotRetrievedWhenCohortsServiceHasFailed()
        {
            //Arrange

            _mediator.Setup(x => x.SendAsync(It.IsAny<GetSingleCohortRequest>()))
               .ReturnsAsync(new GetSingleCohortResponse
               {
                   HasFailed = true
               });

            // Act
            var result = await _sut.GetAccount(HashedAccountId, UserId);

            //Assert
            result.Data.CallToActionViewModel.Should().BeNull();
        }

        [Test]
        public async Task ThenCallToActionDataIsNotRetrievedWhenCohortsServiceHasException()
        {
            //Arrange

            _mediator.Setup(x => x.SendAsync(It.IsAny<GetSingleCohortRequest>()))
               .ThrowsAsync(new Exception());

            // Act
            var result = await _sut.GetAccount(HashedAccountId, UserId);

            //Assert
            result.Data.CallToActionViewModel.Should().BeNull();
        }

        [Test]
        public async Task ThenErrorIsLoggedWhenCohortsServiceHasException()
        {
            //Arrange
            var exception = new Exception();
            _mediator.Setup(x => x.SendAsync(It.IsAny<GetSingleCohortRequest>()))
               .ThrowsAsync(exception);

            // Act
            var result = await _sut.GetAccount(HashedAccountId, UserId);

            //Assert
            _mockLogger.Verify(m => m.Error(exception, $"An error occured whilst trying to retrieve account CallToAction: {HashedAccountId}"), Times.Once);
        }                        

        [Test]
        public async Task ThenShouldGetCohortResponse()
        {
            //Arrange 
            var Cohort = new Cohort() { Id = 1, NumberOfDraftApprentices = 1,  Apprenticeships = new List<Apprenticeship> { new Apprenticeship { FirstName = "FirstName" }  } };            
            _mediator.Setup(x => x.SendAsync(It.IsAny<GetSingleCohortRequest>())).ReturnsAsync(new GetSingleCohortResponse { Cohort = Cohort });            
            var expectedCohort = new CohortViewModel()
            {                
                NumberOfDraftApprentices = 1,
                Apprenticeships = new List<ApprenticeshipViewModel> { new ApprenticeshipViewModel { ApprenticeshipFullName = "FullName" } }
            };            
            _mockMapper.Setup(m => m.Map<Cohort, CohortViewModel>(Cohort)).Returns(expectedCohort);

            //Act
            var result = await _sut.GetAccount(HashedAccountId, UserId);

            //Assert                    
            Assert.AreEqual(1, result.Data.CallToActionViewModel.CohortsCount);            
        }
    }
}
