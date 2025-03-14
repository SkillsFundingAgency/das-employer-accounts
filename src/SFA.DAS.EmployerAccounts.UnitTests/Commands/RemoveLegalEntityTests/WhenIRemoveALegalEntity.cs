﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.EmployerAccounts.Commands.AuditCommand;
using SFA.DAS.EmployerAccounts.Commands.RemoveLegalEntity;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Events.Agreement;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Models.AccountTeam;
using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;
using SFA.DAS.Encoding;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.RemoveLegalEntityTests;

public class WhenIRemoveALegalEntity
{
    private RemoveLegalEntityCommandHandler _handler;
    private Mock<IValidator<RemoveLegalEntityCommand>> _validator;
    private Mock<ILogger<RemoveLegalEntityCommandHandler>> _logger;
    private Mock<IEmployerAgreementRepository> _repository;
    private RemoveLegalEntityCommand _command;
    private Mock<IMediator> _mediator;
    private Mock<IEncodingService> _encodingService;
    private Mock<IMembershipRepository> _membershipRepository;
    private Mock<IEventPublisher> _eventPublisher;
    private Mock<ICommitmentsV2ApiClient> _commitmentsV2ApiClient;

    private const string ExpectedHashedAccountId = "34RFD";
    private const long ExpectedAccountId = 123455;
    private const long ExpectedLegalEntityId = 98854;
    private const string ExpectedLegalEntityName = "Hogwarts";
    private const long ExpectedAccountLegalEntityId = 2017;
    private readonly string _expectedUserId = Guid.NewGuid().ToString();
    private EmployerAgreementView _expectedAgreement;
    private const long ExpectedEmployerAgreementId = 5533678;
    private const string ExpectedHashedEmployerAgreementId = "FGDFH45645";

    [SetUp]
    public void Arrange()
    {
        _validator = new Mock<IValidator<RemoveLegalEntityCommand>>();
        _validator.Setup(x => x.ValidateAsync(It.IsAny<RemoveLegalEntityCommand>())).ReturnsAsync(new ValidationResult());

        _logger = new Mock<ILogger<RemoveLegalEntityCommandHandler>>();
        _mediator = new Mock<IMediator>();

        _repository = new Mock<IEmployerAgreementRepository>();
        _repository.Setup(r => r.GetAccountLegalEntityAgreements(ExpectedAccountLegalEntityId))
            .ReturnsAsync(new List<EmployerAgreement>
            {
                new()
                {
                    AccountLegalEntityId = ExpectedAccountLegalEntityId,
                    TemplateId = 1,
                    Id = ExpectedEmployerAgreementId,
                    SignedDate = DateTime.Now
                }
            });

        _expectedAgreement = new EmployerAgreementView
        {
            AccountLegalEntityId = ExpectedAccountLegalEntityId,
            LegalEntityId = ExpectedLegalEntityId,
            LegalEntityName = ExpectedLegalEntityName,
            Status = EmployerAgreementStatus.Signed,
            LegalEntityCode = "test_code"
        };

        _repository.Setup(r => r.GetEmployerAgreement(ExpectedEmployerAgreementId)).ReturnsAsync(_expectedAgreement);

        _encodingService = new Mock<IEncodingService>();
        _encodingService.Setup(x => x.Encode(ExpectedAccountId, EncodingType.AccountId)).Returns(ExpectedHashedAccountId);
        _encodingService.Setup(x => x.Encode(ExpectedEmployerAgreementId, EncodingType.AccountId)).Returns(ExpectedHashedEmployerAgreementId);
        
        _membershipRepository = new Mock<IMembershipRepository>();
        _membershipRepository
            .Setup(mr => mr.GetCaller(ExpectedAccountId, _expectedUserId))
            .Returns<long, string>((accountId, userRef) => Task.FromResult(new MembershipView { AccountId = ExpectedAccountId, FirstName = "Harry", LastName = "Potter" }));

        _eventPublisher = new Mock<IEventPublisher>();

        _commitmentsV2ApiClient = new Mock<ICommitmentsV2ApiClient>();
        _commitmentsV2ApiClient
            .Setup(x => x.GetEmployerAccountSummary(ExpectedAccountId))
            .ReturnsAsync(new GetApprenticeshipStatusSummaryResponse
            {
                ApprenticeshipStatusSummaryResponse = new List<ApprenticeshipStatusSummaryResponse>
                {
                    new()
                    {
                        ActiveCount = 0,
                        PausedCount = 0,
                        PendingApprovalCount = 0,
                        CompletedCount = 0,
                        LegalEntityIdentifier = _expectedAgreement.LegalEntityCode
                    }
                }
            });

        _command = new RemoveLegalEntityCommand { AccountId = ExpectedAccountId, UserId = _expectedUserId, AccountLegalEntityId = ExpectedAccountLegalEntityId };

        _handler = new RemoveLegalEntityCommandHandler(
            _validator.Object,
            _logger.Object,
            _repository.Object,
            _mediator.Object,
            _encodingService.Object,
            _membershipRepository.Object,
            _eventPublisher.Object,
            _commitmentsV2ApiClient.Object);
    }

    [Test]
    public void ThenTheValidatorIsCalledAndAnInvalidRequestExceptionThrownIfItIsNotValid()
    {
        //Arrange
        _validator.Setup(x => x.ValidateAsync(It.IsAny<RemoveLegalEntityCommand>())).ReturnsAsync(new ValidationResult { ValidationDictionary = new Dictionary<string, string> { { "", "" } } });

        //Act Assert
        var action = () => _handler.Handle(new RemoveLegalEntityCommand(), CancellationToken.None);
        action.Should().ThrowAsync<InvalidRequestException>();
    }

    [Test]
    public void ThenTheValidatorIsCalledAndAnUnauthorizedAccessExceptionIsThrownIfItIsNotAuthorized()
    {
        //Arrange
        _validator.Setup(x => x.ValidateAsync(It.IsAny<RemoveLegalEntityCommand>())).ReturnsAsync(new ValidationResult { IsUnauthorized = true });

        //Act Assert
        var action = () => _handler.Handle(_command, CancellationToken.None);
        action.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Test]
    public async Task ThenTheRepositoryIsCalled()
    {
        //Act
        await _handler.Handle(_command, CancellationToken.None);

        //Assert
        _validator.Verify(x => x.ValidateAsync(It.Is<RemoveLegalEntityCommand>(c =>
            c.AccountId.Equals(ExpectedAccountId)
            && c.UserId.Equals(_expectedUserId))));

        _repository.Verify(x => x.GetAccountLegalEntityAgreements(ExpectedAccountLegalEntityId));
        _repository.Verify(x => x.GetEmployerAgreement(ExpectedEmployerAgreementId));
        _repository.Verify(x => x.RemoveLegalEntityFromAccount(ExpectedEmployerAgreementId));
    }

    [Test]
    public async Task ThenTheAuditIsWrittenToWhenTheItemIsRemoved()
    {
        //Act
        await _handler.Handle(_command, CancellationToken.None);

        //Assert
        _mediator.Verify(x => x.Send(It.Is<CreateAuditCommand>(c =>
            c.EasAuditMessage.ChangedProperties.SingleOrDefault(y => y.PropertyName.Equals("Status") && y.NewValue.Equals(EmployerAgreementStatus.Removed.ToString())) != null
        ), It.IsAny<CancellationToken>()));

        _mediator.Verify(x => x.Send(It.Is<CreateAuditCommand>(c =>
            c.EasAuditMessage.Description.Equals($"EmployerAgreement {ExpectedHashedEmployerAgreementId} removed from account {ExpectedHashedAccountId}")), It.IsAny<CancellationToken>()));

        _mediator.Verify(x => x.Send(It.Is<CreateAuditCommand>(c =>
            c.EasAuditMessage.RelatedEntities.SingleOrDefault(y => y.Id.Equals(ExpectedHashedAccountId) && y.Type.Equals("Account")) != null
        ), It.IsAny<CancellationToken>()));

        _mediator.Verify(x => x.Send(It.Is<CreateAuditCommand>(c =>
            c.EasAuditMessage.AffectedEntity.Id.Equals(ExpectedHashedEmployerAgreementId) &&
            c.EasAuditMessage.AffectedEntity.Type.Equals("EmployerAgreement")
        ), It.IsAny<CancellationToken>()));
    }
    
    [Test]
    public async Task ThenTheRemovedLegalEntityEventIsPublished()
    {
        await _handler.Handle(_command, CancellationToken.None);

        _eventPublisher.Verify(ep => ep.Publish(It.Is<RemovedLegalEntityEvent>(e =>
            e.AccountId.Equals(ExpectedAccountId)
            && e.AgreementId.Equals(ExpectedEmployerAgreementId)
            && e.LegalEntityId.Equals(ExpectedLegalEntityId)
            && e.AgreementSigned.Equals(true)
            && e.OrganisationName.Equals(ExpectedLegalEntityName)
            && e.AccountLegalEntityId.Equals(ExpectedAccountLegalEntityId)
            && e.UserName.Equals("Harry Potter")
            && e.UserRef.Equals(Guid.Parse(_expectedUserId))
        )));
    }

    [Test]
    public void ThenTheAgreementIsCheckedToSeeIfItHasBeenSignedAndHasActiveCommitments()
    {
        _commitmentsV2ApiClient
            .Setup(x => x.GetEmployerAccountSummary(ExpectedAccountId))
            .ReturnsAsync(new GetApprenticeshipStatusSummaryResponse
            {
                ApprenticeshipStatusSummaryResponse = new List<ApprenticeshipStatusSummaryResponse>
                {
                    new()
                    {
                        ActiveCount = 1,
                        PausedCount = 1,
                        PendingApprovalCount = 1,
                        LegalEntityIdentifier = _expectedAgreement.LegalEntityCode
                    }
                }
            });

        var action = () => _handler.Handle(_command, CancellationToken.None);
        action.Should().ThrowAsync<InvalidRequestException>();
    }

    [Test]
    public void ThenTheAgreementIsCheckedToSeeIfItHasBeenSignedAndHasWithdrawnCommitments()
    {
        _commitmentsV2ApiClient
            .Setup(x => x.GetEmployerAccountSummary(ExpectedAccountId))
            .ReturnsAsync(new GetApprenticeshipStatusSummaryResponse
            {
                ApprenticeshipStatusSummaryResponse = new List<ApprenticeshipStatusSummaryResponse>
                {
                    new()
                    {
                        WithdrawnCount = 1,
                        LegalEntityIdentifier = _expectedAgreement.LegalEntityCode
                    }
                }
            });

        var action = () => _handler.Handle(_command, CancellationToken.None);
        action.Should().ThrowAsync<InvalidRequestException>();
    }

    [Test]
    public void ThenTheAgreementIsCheckedToSeeIfItHasBeenSignedAndHasCompletedCommitments()
    {
        _commitmentsV2ApiClient
            .Setup(x => x.GetEmployerAccountSummary(ExpectedAccountId))
            .ReturnsAsync(new GetApprenticeshipStatusSummaryResponse
            {
                ApprenticeshipStatusSummaryResponse = new List<ApprenticeshipStatusSummaryResponse>
                {
                    new()
                    {
                        CompletedCount = 1,
                        LegalEntityIdentifier = _expectedAgreement.LegalEntityCode
                    }
                }
            });

        var action = () => _handler.Handle(_command, CancellationToken.None);
        action.Should().ThrowAsync<InvalidRequestException>();
    }
}