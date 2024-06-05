using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.SupportCreateInvitation;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Models.AccountTeam;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.SupportCreateInvitationTests;

public class WhenIValidateSupportCreateInvitation
{
    private SupportCreateInvitationCommandValidator _validator;
    private SupportCreateInvitationCommand _createInvitationCommand;
    private Mock<IMembershipRepository> _membershipRepository;
    private Mock<IEncodingService> _encodingService;
    private const long AccountId = 122;

    [SetUp]
    public void Arrange()
    {
        _createInvitationCommand = new SupportCreateInvitationCommand
        {
            EmailOfPersonBeingInvited = "so'me@email.com",
            SupportUserEmail = "support@email.test",
            HashedAccountId = "123dfg",
            NameOfPersonBeingInvited = "Test",
            RoleOfPersonBeingInvited = Role.Owner
        };

        _membershipRepository = new Mock<IMembershipRepository>();
        _membershipRepository.Setup(x => x.GetCaller(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new MembershipView { Role = Role.Owner });
        _membershipRepository.Setup(x => x.Get(It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(new TeamMember { IsUser = false });

        _encodingService = new Mock<IEncodingService>();
        _encodingService.Setup(x => x.Decode(_createInvitationCommand.HashedAccountId, EncodingType.AccountId)).Returns(AccountId);

        _validator = new SupportCreateInvitationCommandValidator(_membershipRepository.Object, _encodingService.Object);
    }

    [Test]
    public async Task ThenTrueIsReturnedIfAllFieldsArePopulated()
    {
        //Act
        var result = await _validator.ValidateAsync(_createInvitationCommand);

        //Assert
        result.IsValid().Should().BeTrue();
    }

    [Test]
    public async Task ThenTheErrorDictionaryIsPopulatedWithErrorsWhenInvalid()
    {
        //Act
        var result = await _validator.ValidateAsync(new SupportCreateInvitationCommand());

        //Assert
        result.IsValid().Should().BeFalse();
        result.ValidationDictionary.Should().Contain(new KeyValuePair<string, string>("EmailOfPersonBeingInvited", "Enter email address"));
        result.ValidationDictionary.Should().Contain(new KeyValuePair<string, string>("HashedAccountId", "No HashedAccountId supplied"));
        result.ValidationDictionary.Should().Contain(new KeyValuePair<string, string>("NameOfPersonBeingInvited", "Enter name"));
        result.ValidationDictionary.Should().Contain(new KeyValuePair<string, string>("SupportUserEmail", "Specify support user email"));
    }

    [TestCase("notvalid")]
    [TestCase("notvalid.com")]
    [TestCase("notvalid@valid")]
    public async Task ThenTheEmailFormatIsValidatedWhenPopulatedAndReturnsFalseForNonValidEmails(string email)
    {
        //Act
        var result = await _validator.ValidateAsync(new SupportCreateInvitationCommand
        {
            EmailOfPersonBeingInvited = email,
            SupportUserEmail = "support@email.test",
            HashedAccountId = "123dfg",
            NameOfPersonBeingInvited = "Test",
            RoleOfPersonBeingInvited = Role.Owner
        });

        //Assert
        result.IsValid().Should().BeFalse();
        result.ValidationDictionary.Should().Contain(new KeyValuePair<string, string>("EmailOfPersonBeingInvited", "Enter a valid email address"));
    }

    [Test]
    public async Task ThenFalseIsReturnedIfTheEmailIsAlreadyInUse()
    {
        //Arrange
        _membershipRepository.Setup(x => x.Get(It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(new TeamMember { IsUser = true });

        //Act
        var result = await _validator.ValidateAsync(_createInvitationCommand);

        //Assert
        result.IsValid().Should().BeFalse();
        result.ValidationDictionary.Should().Contain(new KeyValuePair<string, string>("EmailOfPersonBeingInvited", $"{_createInvitationCommand.EmailOfPersonBeingInvited} is already invited"));
    }
}