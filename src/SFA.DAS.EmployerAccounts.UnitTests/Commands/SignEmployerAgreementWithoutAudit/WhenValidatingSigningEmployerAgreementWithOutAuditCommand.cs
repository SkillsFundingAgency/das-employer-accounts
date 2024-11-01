using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.SignEmployerAgreementWithOutAudit;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.EmployerAgreement;
using SFA.DAS.EmployerAccounts.Models.UserProfile;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.SignEmployerAgreementWithoutAudit;

public class WhenValidatingSigningEmployerAgreementWithOutAuditCommand
{
    private SignEmployerAgreementWithoutAuditCommandValidator _sut;
    private Mock<IEmployerAgreementRepository> _employerAgreementRepository;
    private Mock<IEncodingService> _encodingService;

    [SetUp]
    public void Arrange()
    {
        _employerAgreementRepository = new Mock<IEmployerAgreementRepository>();
        _encodingService = new Mock<IEncodingService>();

        _sut = new SignEmployerAgreementWithoutAuditCommandValidator(_employerAgreementRepository.Object, _encodingService.Object);
    }

    [Test]
    public async Task ThenIfAllFieldsArePopulatedAndTheAgreementIsPendingTheRequestIsValid()
    {
        //Arrange
        const long employerAgreementId = 12345;
        const string hashedAgreementId = "123ASD";

        _encodingService.Setup(x => x.Decode(hashedAgreementId, EncodingType.AccountId)).Returns(employerAgreementId);
        _employerAgreementRepository.Setup(x => x.GetEmployerAgreementStatus(employerAgreementId)).ReturnsAsync(EmployerAgreementStatus.Pending);

        //Act
        var actual = await _sut.ValidateAsync(new SignEmployerAgreementWithoutAuditCommand(hashedAgreementId, new User(), Guid.NewGuid().ToString()));

        //Assert
        Assert.That(actual.IsValid(), Is.True);
    }

    [TestCase(EmployerAgreementStatus.Signed)]
    [TestCase(EmployerAgreementStatus.Expired)]
    [TestCase(EmployerAgreementStatus.Superseded)]
    public async Task ThenIfTheAgreementIsAlreadySignedThenTheRequestIsNotValid(EmployerAgreementStatus employerAgreementStatus)
    {
        //Arrange
        var employerAgreementId = 12345;
        var hashedAgreementId = "123ASD";
        _encodingService.Setup(x => x.Decode(hashedAgreementId, EncodingType.AccountId)).Returns(employerAgreementId);
        _employerAgreementRepository.Setup(x => x.GetEmployerAgreementStatus(employerAgreementId)).ReturnsAsync(employerAgreementStatus);

        //Act
        var actual = await _sut.ValidateAsync(new SignEmployerAgreementWithoutAuditCommand(hashedAgreementId, new User(), Guid.NewGuid().ToString()));

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(actual.IsValid(), Is.False);
            Assert.That(actual.ValidationDictionary, Has.Count.EqualTo(1));
            Assert.That(actual.ValidationDictionary.First().Key, Is.EqualTo("employerAgreementStatus"));
        });
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public async Task ThenIfTheHashedAgreementIdIsNotGivenThenRequestIsNotValid(string hashedAgreementId)
    {
        //Act
        var actual = await _sut.ValidateAsync(new SignEmployerAgreementWithoutAuditCommand(hashedAgreementId, new User(), Guid.NewGuid().ToString()));

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(actual.IsValid(), Is.False);
            Assert.That(actual.ValidationDictionary, Has.Count.EqualTo(1));
            Assert.That(actual.ValidationDictionary.First().Key, Is.EqualTo(nameof(SignEmployerAgreementWithoutAuditCommand.HashedAgreementId)));
        });
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public async Task ThenIfTheCorrelationIdIsNotGivenThenRequestIsNotValid(string correlationId)
    {
        //Arrange
        var employerAgreementId = 12345;
        var hashedAgreementId = "123ASD";
        _encodingService.Setup(x => x.Decode(hashedAgreementId, EncodingType.AccountId)).Returns(employerAgreementId);
        _employerAgreementRepository.Setup(x => x.GetEmployerAgreementStatus(employerAgreementId)).ReturnsAsync(EmployerAgreementStatus.Pending);

        //Act
        var actual = await _sut.ValidateAsync(new SignEmployerAgreementWithoutAuditCommand(hashedAgreementId, new User(), correlationId));

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(actual.IsValid(), Is.False);
            Assert.That(actual.ValidationDictionary, Has.Count.EqualTo(1));
            Assert.That(actual.ValidationDictionary.First().Key, Is.EqualTo(nameof(SignEmployerAgreementWithoutAuditCommand.CorrelationId)));
        });
    }

    [Test]
    public async Task ThenIfTheUserIsNotGivenThenRequestIsNotValid()
    {
        //Arrange
        var employerAgreementId = 12345;
        var hashedAgreementId = "123ASD";
        _encodingService.Setup(x => x.Decode(hashedAgreementId, EncodingType.AccountId)).Returns(employerAgreementId);
        _employerAgreementRepository.Setup(x => x.GetEmployerAgreementStatus(employerAgreementId)).ReturnsAsync(EmployerAgreementStatus.Pending);

        //Act
        var actual = await _sut.ValidateAsync(new SignEmployerAgreementWithoutAuditCommand(hashedAgreementId, null, Guid.NewGuid().ToString()));

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(actual.IsValid(), Is.False);
            Assert.That(actual.ValidationDictionary, Has.Count.EqualTo(1));
            Assert.That(actual.ValidationDictionary.First().Key, Is.EqualTo(nameof(SignEmployerAgreementWithoutAuditCommand.User)));
        });
    }

    [Test]
    public async Task ThenIfTheAgreementIsNotFoundThenRequestIsNotValid()
    {
        //Arrange
        var employerAgreementId = 12345;
        var hashedAgreementId = "123ASD";
        _encodingService.Setup(x => x.Decode(hashedAgreementId, EncodingType.AccountId)).Returns(employerAgreementId);
        _employerAgreementRepository.Setup(x => x.GetEmployerAgreementStatus(employerAgreementId)).ReturnsAsync(() => null);

        //Act
        var actual = await _sut.ValidateAsync(new SignEmployerAgreementWithoutAuditCommand(hashedAgreementId, new User(), Guid.NewGuid().ToString()));

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(actual.IsValid(), Is.False);
            Assert.That(actual.ValidationDictionary, Has.Count.EqualTo(1));
            Assert.That(actual.ValidationDictionary.First().Key, Is.EqualTo("employerAgreementStatus"));
        });
    }
}
