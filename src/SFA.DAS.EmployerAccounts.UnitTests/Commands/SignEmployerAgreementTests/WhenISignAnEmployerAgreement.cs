using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Commands.SignEmployerAgreement;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Models.AccountTeam;
using SFA.DAS.EmployerAccounts.Models.CommitmentsV2;
using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;
using SFA.DAS.EmployerAccounts.Models.UserProfile;
using SFA.DAS.EmployerAccounts.Queries.GetUserByRef;
using SFA.DAS.Encoding;
using SFA.DAS.NServiceBus.Testing.Services;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.SignEmployerAgreementTests;

[TestFixture]
public class WhenISignAnEmployerAgreement
{
    private Mock<IMembershipRepository> _membershipRepository;
    private Mock<IEmployerAgreementRepository> _agreementRepository;
    private SignEmployerAgreementCommandHandler _handler;
    private SignEmployerAgreementCommand _command;
    private MembershipView _owner;
    private Mock<IEncodingService> _encodingService;
    private Mock<IValidator<SignEmployerAgreementCommand>> _validator;
    private Mock<IMediator> _mediator;
    private EmployerAgreementView _agreement;
    private Mock<ICommitmentV2Service> _commitmentService;
    private TestableEventPublisher _eventPublisher;

    private const long AccountId = 223344;
    private const long AgreementId = 123433;
    private const long LegalEntityId = 111333;
    private const string OrganisationName = "Foo";
    private const string HashedLegalEntityId = "2635JHG";
    private const long AccountLegalEntityId = 9568456;
    private const AgreementType AgreementType = Common.Domain.Types.AgreementType.Combined;

    [SetUp]
    public void Setup()
    {
        _command = new SignEmployerAgreementCommand
        {
            HashedAccountId = "1AVCFD",
            HashedAgreementId = "2EQWE34",
            ExternalUserId = Guid.NewGuid().ToString(),
            SignedDate = DateTime.Now
        };

        _membershipRepository = new Mock<IMembershipRepository>();

        _encodingService = new Mock<IEncodingService>();
        _encodingService.Setup(x => x.Decode(_command.HashedAccountId, EncodingType.AccountId)).Returns(AccountId);
        _encodingService.Setup(x => x.Decode(_command.HashedAgreementId, EncodingType.AccountId)).Returns(AgreementId);
        _encodingService.Setup(x => x.Encode(LegalEntityId, EncodingType.AccountId)).Returns(HashedLegalEntityId);

        _validator = new Mock<IValidator<SignEmployerAgreementCommand>>();
        _validator.Setup(x => x.ValidateAsync(It.IsAny<SignEmployerAgreementCommand>())).ReturnsAsync(new ValidationResult { ValidationDictionary = new Dictionary<string, string>() });

        _agreement = new EmployerAgreementView
        {
            LegalEntityId = LegalEntityId,
            LegalEntityName = OrganisationName,
            AgreementType = AgreementType,
            AccountId = AccountId,
            AccountLegalEntityId = AccountLegalEntityId,
            Id = AgreementId
        };

        _agreementRepository = new Mock<IEmployerAgreementRepository>();

        _agreementRepository.Setup(x => x.GetEmployerAgreement(It.IsAny<long>()))
            .ReturnsAsync(_agreement);

        _mediator = new Mock<IMediator>();

        _mediator.Setup(x => x.Send(It.Is<GetUserByRefQuery>(s => s.UserRef == _command.ExternalUserId), It.IsAny<CancellationToken>())).ReturnsAsync(new GetUserByRefResponse { User = new User { CorrelationId = "CORRELATION_ID" } });

        _commitmentService = new Mock<ICommitmentV2Service>();

        _commitmentService.Setup(x => x.GetEmployerCommitments(It.IsAny<long>()))
            .ReturnsAsync([]);

        _eventPublisher = new TestableEventPublisher();

        _handler = new SignEmployerAgreementCommandHandler(
            _membershipRepository.Object,
            _agreementRepository.Object,
            _encodingService.Object,
            _validator.Object,
            _mediator.Object,
            _eventPublisher,
            _commitmentService.Object);

        _owner = new MembershipView
        {
            UserId = 1,
            Role = Role.Owner,
            FirstName = "Fred",
            LastName = "Bloggs",
            UserRef = Guid.NewGuid()
        };

        _membershipRepository.Setup(x => x.GetCaller(_command.HashedAccountId, _command.ExternalUserId))
            .ReturnsAsync(_owner);
    }

    [Test]
    public void ThenTheValidatorIsCalledAndAnInvalidRequestExceptionIsThrownIfItIsNotValid()
    {
        //Arrange
        _validator.Setup(x => x.ValidateAsync(It.IsAny<SignEmployerAgreementCommand>())).ReturnsAsync(new ValidationResult { ValidationDictionary = new Dictionary<string, string> { { "", "" } } });

        //Act Assert
        Assert.ThrowsAsync<InvalidRequestException>(async () => await _handler.Handle(_command, CancellationToken.None));
    }

    [Test]
    public void ThenIfTheUserIsNotConnectedToTheAccountThenAnUnauthorizedExceptionIsThrown()
    {
        //Arrange
        _membershipRepository.Setup(x => x.GetCaller(_command.HashedAccountId, _command.ExternalUserId)).ReturnsAsync(() => null);

        //Act Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _handler.Handle(_command, CancellationToken.None));
    }

    [TestCase(Role.Transactor)]
    [TestCase(Role.Viewer)]
    [TestCase(Role.None)]
    public void ThenIfTheUserIsNotAnOwnerThenAnUnauthorizedExceptionIsThrown(Role role)
    {
        //Arrange
        _membershipRepository.Setup(x => x.GetCaller(_command.HashedAccountId, _command.ExternalUserId)).ReturnsAsync(new MembershipView { Role = role });

        //Act Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _handler.Handle(_command, CancellationToken.None));
    }

    [Test]
    public async Task ThenIfTheCommandIsValidTheRepositoryIsCalledWithThePassedParameters()
    {
        //Arrange
        const int agreementId = 87761263;
        _encodingService.Setup(x => x.Decode(_command.HashedAgreementId, EncodingType.AccountId)).Returns(agreementId);

        //Act
        var response = await _handler.Handle(_command, CancellationToken.None);

        //Assert
        _agreementRepository.Verify(x => x.SignAgreement(It.Is<SignEmployerAgreement>(c => c.SignedDate.Equals(_command.SignedDate)
                                                                                           && c.AgreementId.Equals(agreementId)
                                                                                           && c.SignedDate.Equals(_command.SignedDate)
                                                                                           && c.SignedById.Equals(_owner.UserId)
                                                                                           && c.SignedByName.Equals($"{_owner.FirstName} {_owner.LastName}")
        )));

        Assert.That(response.LegalEntityName, Is.EqualTo(OrganisationName));
        Assert.That(response.AgreementType, Is.EqualTo(AgreementType));
    }

    [Test]
    public async Task ThenIfTheCommandIsValidTheAccountLegalEntityAgreementDetailsShouldBeUpdated()
    {
        //Act
        await _handler.Handle(_command, CancellationToken.None);

        //Assert
        _agreementRepository.Verify(x => x.SetAccountLegalEntityAgreementDetails(_agreement.AccountLegalEntityId, (long?)null, (int?)null, _agreement.Id, _agreement.VersionNumber));
    }
    
    [Test]
    public async Task ThenTheServiceShouldBeNotified()
    {
        //Arrange
        _commitmentService.Setup(x => x.GetEmployerCommitments(It.IsAny<long>()))
            .ReturnsAsync([new Cohort()]);
        
        //Act
        await _handler.Handle(_command, CancellationToken.None);

        //Assert
        _eventPublisher.Events.Should().HaveCount(1);

        var message = _eventPublisher.Events.First().As<SignedAgreementEvent>();

        message.AccountId.Should().Be(AccountId);
        message.AgreementId.Should().Be(AgreementId);
        message.OrganisationName.Should().Be(OrganisationName);
        message.AccountLegalEntityId.Should().Be(AccountLegalEntityId);
        message.LegalEntityId.Should().Be(LegalEntityId);
        message.CohortCreated.Should().BeTrue();
        message.UserName.Should().Be(_owner.FullName());
        message.UserRef.Should().Be(_owner.UserRef);
        message.AgreementType.Should().Be(AgreementType);
    }

    [Test]
    public async Task ThenIfICannotGetCommitmentsForTheAccountIStillNotifyTheService()
    {
        //Arrange
        _commitmentService.Setup(x => x.GetEmployerCommitments(It.IsAny<long>()))
            .ReturnsAsync(() => null);

        //Act
        await _handler.Handle(_command, CancellationToken.None);

        //Assert
        _eventPublisher.Events.Should().HaveCount(1);

        var message = _eventPublisher.Events.First().As<SignedAgreementEvent>();

        message.AccountId.Should().Be(AccountId);
        message.AgreementId.Should().Be(AgreementId);
        message.OrganisationName.Should().Be(OrganisationName);
        message.AccountLegalEntityId.Should().Be(AccountLegalEntityId);
        message.LegalEntityId.Should().Be(LegalEntityId);
        message.CohortCreated.Should().BeFalse();
        message.UserName.Should().Be(_owner.FullName());
        message.UserRef.Should().Be(_owner.UserRef);
        message.AgreementType.Should().Be(AgreementType);
    }
}