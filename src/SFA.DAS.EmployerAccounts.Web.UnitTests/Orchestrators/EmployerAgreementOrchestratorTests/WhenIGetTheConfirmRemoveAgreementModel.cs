using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;
using SFA.DAS.EmployerAccounts.Queries.GetAccountEmployerAgreementRemove;
using SFA.DAS.EmployerAccounts.Web.Orchestrators;
using SFA.DAS.Validation;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Orchestrators.EmployerAgreementOrchestratorTests
{
    public class WhenIGetTheConfirmRemoveAgreementModel
    {
        private Mock<IMediator> _mediator;
        private Mock<IReferenceDataService> _referenceDataService;
        private EmployerAgreementOrchestrator _orchestrator;

        private const string ExpectedHahsedAccountId = "RT456";
        private const string ExpectedHashedAgreementId = "RRTE56";
        private const string ExpectedUserId = "TYG68UY";
        private const string ExpectedName = "Test Name";

        [SetUp]
        public void Arrange()
        {
            _mediator = new Mock<IMediator>();
            _mediator.Setup(x => x.SendAsync(It.IsAny<GetAccountEmployerAgreementRemoveRequest>()))
                .ReturnsAsync(new GetAccountEmployerAgreementRemoveResponse
                {
                    Agreement = new RemoveEmployerAgreementView
                    {
                        Name = ExpectedName,
                        CanBeRemoved = false,
                        HashedAccountId = ExpectedHahsedAccountId,
                        HashedAgreementId = ExpectedHashedAgreementId,
                        Id = 123444
                    }

                });

            _referenceDataService = new Mock<IReferenceDataService>();

            _orchestrator = new EmployerAgreementOrchestrator(_mediator.Object, Mock.Of<IMapper>(), _referenceDataService.Object);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledToGetASingledOrgToRemove()
        {

            //Act
            await _orchestrator.GetConfirmRemoveOrganisationViewModel(ExpectedHashedAgreementId, ExpectedHahsedAccountId, ExpectedUserId);

            //Assert
            _mediator.Verify(x => x.SendAsync(It.Is<GetAccountEmployerAgreementRemoveRequest>(
                c => c.HashedAccountId.Equals(ExpectedHahsedAccountId)
                     && c.UserId.Equals(ExpectedUserId)
                     && c.HashedAgreementId.Equals(ExpectedHashedAgreementId)
                )), Times.Once);
        }


        [Test]
        public async Task ThenIfAnInvalidRequestExceptionIsThrownTheOrchestratorResponseContainsTheError()
        {
            //Arrange
            _mediator.Setup(x => x.SendAsync(It.IsAny<GetAccountEmployerAgreementRemoveRequest>())).ThrowsAsync(new InvalidRequestException(new Dictionary<string, string>()));

            //Act
            var actual = await _orchestrator.GetConfirmRemoveOrganisationViewModel(ExpectedHashedAgreementId, ExpectedHahsedAccountId, ExpectedUserId);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, actual.Status);
        }

        [Test]
        public async Task ThenIfAUnauthroizedAccessExceptionIsThrownThenTheOrchestratorResponseShowsAccessDenied()
        {
            //Arrange
            _mediator.Setup(x => x.SendAsync(It.IsAny<GetAccountEmployerAgreementRemoveRequest>())).ThrowsAsync(new UnauthorizedAccessException());

            //Act
            var actual = await _orchestrator.GetConfirmRemoveOrganisationViewModel(ExpectedHashedAgreementId, ExpectedHahsedAccountId, ExpectedUserId);

            //Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, actual.Status);
        }

        [Test]
        public async Task ThenTheValuesAreReturnedInTheResponseFromTheMediatorCall()
        {
            //Act
            var actual = await _orchestrator.GetConfirmRemoveOrganisationViewModel(ExpectedHashedAgreementId, ExpectedHahsedAccountId, ExpectedUserId);

            //Assert
            Assert.AreEqual(ExpectedHashedAgreementId, actual.Data.HashedAgreementId);
            Assert.AreEqual(ExpectedHahsedAccountId, actual.Data.HashedAccountId);
            Assert.AreEqual(ExpectedName, actual.Data.Name);
        }

    }
}