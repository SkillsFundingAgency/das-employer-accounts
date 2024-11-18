using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.SignEmployerAgreementWithOutAudit;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.SignEmployerAgreementWithoutAudit;

public class WhenSigningEmployerAgreementWithOutAudit
{
    private Mock<IEmployerAgreementRepository> _employerAgreementRepositoryMock;
    private Mock<IEventPublisher> _eventPublisherMock;
    private Mock<IValidator<SignEmployerAgreementWithoutAuditCommand>> _validatorMock;
    private SignEmployerAgreementWithoutAuditCommandHandler _sut;

    [SetUp]
    public void Initialize()
    {
        _employerAgreementRepositoryMock = new();
        _eventPublisherMock = new();
        _validatorMock = new();

        _sut = new(_employerAgreementRepositoryMock.Object, _eventPublisherMock.Object, _validatorMock.Object);
    }

    [Test, AutoData]
    public async Task ThenIfRequestIsInvalidThenThrowsInvalidRequestException(SignEmployerAgreementWithoutAuditCommand command)
    {
        var validationResult = new ValidationResult() { ValidationDictionary = { { "field", "error" } } };
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<SignEmployerAgreementWithoutAuditCommand>())).ReturnsAsync(validationResult);

        Func<Task> action = () => _sut.Handle(command, CancellationToken.None);

        await action.Should().ThrowAsync<InvalidRequestException>();
    }

    [Test, AutoData]
    public async Task ThenIfRequestIsValidThenSignsAgreementAndPublishesEvent(SignEmployerAgreementWithoutAuditCommand command, EmployerAgreementView agreement)
    {
        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<SignEmployerAgreementWithoutAuditCommand>())).ReturnsAsync(new ValidationResult());

        _employerAgreementRepositoryMock.Setup(r => r.GetEmployerAgreement(command.AgreementId)).ReturnsAsync(agreement);

        await _sut.Handle(command, CancellationToken.None);

        _employerAgreementRepositoryMock.Verify(r => r.SignAgreement(It.Is<SignEmployerAgreement>(a => a.AgreementId == command.AgreementId && a.SignedById == command.User.Id && a.SignedByName == command.User.FullName)));

        _employerAgreementRepositoryMock.Verify(r => r.SetAccountLegalEntityAgreementDetails(agreement.AccountLegalEntityId, null, null, agreement.Id, agreement.VersionNumber, false), Times.Once);

        _eventPublisherMock.Verify(p => p.Publish(It.Is<SignedAgreementEvent>(e =>
            e.AgreementId == command.AgreementId &&
            e.AccountId == agreement.AccountId &&
            e.AccountLegalEntityId == agreement.AccountLegalEntityId &&
            e.LegalEntityId == agreement.LegalEntityId &&
            e.OrganisationName == agreement.LegalEntityName &&
            !e.CohortCreated &&
            e.UserName == command.User.FullName &&
            e.UserRef == command.User.Ref &&
            e.AgreementType == agreement.AgreementType &&
            e.SignedAgreementVersion == agreement.VersionNumber &&
            e.CorrelationId == command.CorrelationId)));
    }
}
