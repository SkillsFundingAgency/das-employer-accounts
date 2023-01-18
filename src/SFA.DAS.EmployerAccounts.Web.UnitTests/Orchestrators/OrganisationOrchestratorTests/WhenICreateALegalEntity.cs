﻿using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Commands.CreateLegalEntity;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.MarkerInterfaces;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;
using SFA.DAS.EmployerAccounts.Web.Orchestrators;
using SFA.DAS.EmployerAccounts.Web.ViewModels;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EmployerAccounts.Web.UnitTests.Orchestrators.OrganisationOrchestratorTests
{
    class WhenICreateALegalEntity
    {
        private OrganisationOrchestrator _orchestrator;
        private Mock<IMediator> _mediator;
        private Mock<IMapper> _mapper;
        private Mock<IAccountLegalEntityPublicHashingService> _hashingService;
        private Mock<ICookieStorageService<EmployerAccountData>> _cookieService;

        [SetUp]
        public void Arrange()
        {
            _mediator = new Mock<IMediator>();
            _mapper = new Mock<IMapper>();
            _hashingService = new Mock<IAccountLegalEntityPublicHashingService>();
            _cookieService = new Mock<ICookieStorageService<EmployerAccountData>>();

            _orchestrator = new OrganisationOrchestrator(
                _mediator.Object, 
                _mapper.Object,
                _cookieService.Object, 
                _hashingService.Object);
        }

        [Test]
        public async Task ThenTheLegalEntityShouldBeCreated()
        {
            //Assign
            var request = new CreateNewLegalEntityViewModel
            {
                HashedAccountId = "1",
                Name = "Test Corp",
                Code = "SD665734",
                Source = OrganisationType.CompaniesHouse,
                Address = "1, Test Street",
                IncorporatedDate = DateTime.Now.AddYears(-20),
                ExternalUserId = "2",
                LegalEntityStatus = "active"
            };

            const long legalEntityId = 5;
            const long agreementEntityId = 6;

            _mediator.Setup(x => x.Send(It.IsAny<CreateLegalEntityCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new CreateLegalEntityCommandResponse
                     {
                         AgreementView = new EmployerAgreementView
                         {
                             Id = agreementEntityId,
                             LegalEntityId = legalEntityId,
                             LegalEntityName = request.Name,
                             LegalEntityCode = request.Code,
                             LegalEntitySource = request.Source,
                             LegalEntityAddress = request.Address,
                             LegalEntityStatus = request.LegalEntityStatus,
                             Status = EmployerAgreementStatus.Pending
                         }
                     });

            //Act
            await _orchestrator.CreateLegalEntity(request);

            //Assert
            _mediator.Verify(x => x.Send(It.Is<CreateLegalEntityCommand>(command =>
            command.Name.Equals(request.Name) &&
            command.Address.Equals(request.Address) &&
            command.Code.Equals(request.Code) &&
            command.Source.Equals(request.Source) &&
            command.DateOfIncorporation.Equals(request.IncorporatedDate) &&
            command.Status.Equals(request.LegalEntityStatus)), It.IsAny<CancellationToken>()));
        }
    }
}
