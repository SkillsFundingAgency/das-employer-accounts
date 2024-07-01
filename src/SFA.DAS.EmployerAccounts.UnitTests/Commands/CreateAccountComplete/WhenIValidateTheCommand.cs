using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.CreateAccountComplete;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.CreateAccountComplete;

public class WhenIValidateTheCommand
{
    private SendAccountTaskListCompleteNotificationCommandValidator _createCommandValidator;
    private SendAccountTaskListCompleteNotificationCommand _createAccountCommand;

    [SetUp]
    public void Arrange()
    {
        _createAccountCommand = new SendAccountTaskListCompleteNotificationCommand
        {
            HashedAccountId = "ABC123",
            OrganisationName = "ZZZAAA",
            ExternalUserId = "11122444"
        };

        _createCommandValidator = new SendAccountTaskListCompleteNotificationCommandValidator();
    }

    [Test]
    public void ThenAllFieldsAreValidatedToSeeIfTheyHaveBeenPopulated()
    {
        //Act
        var actual =  _createCommandValidator.Validate(_createAccountCommand);

        //Assert
        Assert.That(actual.IsValid(), Is.True);
    }

    [Test]
    public void ThenFalseIsReturnedIfThefieldsArentPopulated()
    {
        //Act
        var actual = _createCommandValidator.Validate(new SendAccountTaskListCompleteNotificationCommand());

        //Assert
        Assert.That(actual.IsValid(), Is.False);
    }
}