using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.SignEmployerAgreementWithOutAudit;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;
using SFA.DAS.EmployerAccounts.Models.UserProfile;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.SignEmployerAgreementWithoutAudit;

public class WhenValidatingSigningEmployerAgreementWithOutAuditCommand
{
    private SignEmployerAgreementWithoutAuditCommandValidator _sut;
    private Mock<IEmployerAgreementRepository> _employerAgreementRepository;

    [SetUp]
    public void Arrange()
    {
        _employerAgreementRepository = new Mock<IEmployerAgreementRepository>();

        _sut = new SignEmployerAgreementWithoutAuditCommandValidator(_employerAgreementRepository.Object);
    }

    [Test]
    public async Task ThenIfAllFieldsArePopulatedAndTheAgreementIsPendingTheRequestIsValid()
    {
        //Arrange
        const long employerAgreementId = 12345;

        _employerAgreementRepository.Setup(x => x.GetEmployerAgreementStatus(employerAgreementId)).ReturnsAsync(EmployerAgreementStatus.Pending);

        //Act
        var actual = await _sut.ValidateAsync(new SignEmployerAgreementWithoutAuditCommand(employerAgreementId, new User(), Guid.NewGuid().ToString()));

        //Assert
        actual.IsValid().Should().BeTrue();
    }

    [TestCase(EmployerAgreementStatus.Signed)]
    [TestCase(EmployerAgreementStatus.Expired)]
    [TestCase(EmployerAgreementStatus.Superseded)]
    public async Task ThenIfTheAgreementIsAlreadySignedThenTheRequestIsNotValid(EmployerAgreementStatus employerAgreementStatus)
    {
        //Arrange
        var employerAgreementId = 12345;
        _employerAgreementRepository.Setup(x => x.GetEmployerAgreementStatus(employerAgreementId)).ReturnsAsync(employerAgreementStatus);

        //Act
        var actual = await _sut.ValidateAsync(new SignEmployerAgreementWithoutAuditCommand(employerAgreementId, new User(), Guid.NewGuid().ToString()));

        //Assert
        using (new AssertionScope())
        {
            actual.IsValid().Should().BeFalse();
            actual.ValidationDictionary.Should().HaveCount(1);
            actual.ValidationDictionary.First().Key.Should().Be("employerAgreementStatus");
        }
    }

    [TestCase(0)]
    [TestCase(-1)]
    public async Task ThenIfTheHashedAgreementIdIsNotGivenThenRequestIsNotValid(long agreementId)
    {
        //Act
        var actual = await _sut.ValidateAsync(new SignEmployerAgreementWithoutAuditCommand(agreementId, new User(), Guid.NewGuid().ToString()));

        //Assert
        using (new AssertionScope())
        {
            actual.IsValid().Should().BeFalse();
            actual.ValidationDictionary.Should().HaveCount(1);
            actual.ValidationDictionary.First().Key.Should().Be(nameof(SignEmployerAgreementWithoutAuditCommand.AgreementId));
        }
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public async Task ThenIfTheCorrelationIdIsNotGivenThenRequestIsNotValid(string correlationId)
    {
        //Arrange
        var employerAgreementId = 12345;
        _employerAgreementRepository.Setup(x => x.GetEmployerAgreementStatus(employerAgreementId)).ReturnsAsync(EmployerAgreementStatus.Pending);

        //Act
        var actual = await _sut.ValidateAsync(new SignEmployerAgreementWithoutAuditCommand(employerAgreementId, new User(), correlationId));

        //Assert
        using (new AssertionScope())
        {
            actual.IsValid().Should().BeFalse();
            actual.ValidationDictionary.Should().HaveCount(1);
            actual.ValidationDictionary.First().Key.Should().Be(nameof(SignEmployerAgreementWithoutAuditCommand.CorrelationId));
        }
    }

    [Test]
    public async Task ThenIfTheUserIsNotGivenThenRequestIsNotValid()
    {
        //Arrange
        var employerAgreementId = 12345;
        _employerAgreementRepository.Setup(x => x.GetEmployerAgreementStatus(employerAgreementId)).ReturnsAsync(EmployerAgreementStatus.Pending);

        //Act
        var actual = await _sut.ValidateAsync(new SignEmployerAgreementWithoutAuditCommand(employerAgreementId, null, Guid.NewGuid().ToString()));

        //Assert
        using (new AssertionScope())
        {
            actual.IsValid().Should().BeFalse();
            actual.ValidationDictionary.Should().HaveCount(1);
            actual.ValidationDictionary.First().Key.Should().Be(nameof(SignEmployerAgreementWithoutAuditCommand.User));
        }
    }

    [Test]
    public async Task ThenIfTheAgreementIsNotFoundThenRequestIsNotValid()
    {
        //Arrange
        var employerAgreementId = 12345;
        _employerAgreementRepository.Setup(x => x.GetEmployerAgreementStatus(employerAgreementId)).ReturnsAsync(() => null);

        //Act
        var actual = await _sut.ValidateAsync(new SignEmployerAgreementWithoutAuditCommand(employerAgreementId, new User(), Guid.NewGuid().ToString()));

        //Assert
        using (new AssertionScope())
        {
            actual.IsValid().Should().BeFalse();
            actual.ValidationDictionary.Should().HaveCount(1);
            actual.ValidationDictionary.First().Key.Should().Be("employerAgreementStatus");
        }
    }
}
