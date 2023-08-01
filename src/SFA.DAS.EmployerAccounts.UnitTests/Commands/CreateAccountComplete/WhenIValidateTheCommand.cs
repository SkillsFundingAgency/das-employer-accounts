using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.CreateAccountComplete;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.CreateAccountComplete;

public class WhenIValidateTheCommand
{
    private CreateAccountCompleteCommandValidator _createCommandValidator;
    private CreateAccountCompleteCommand _createAccountCommand;

    [SetUp]
    public void Arrange()
    {
        _createAccountCommand = new CreateAccountCompleteCommand
        {
            HashedAccountId = "ABC123",
            OrganisationName = "ZZZAAA",
            ExternalUserId = "11122444"
        };

        _createCommandValidator = new CreateAccountCompleteCommandValidator();
    }

    [Test]
    public void ThenAllFieldsAreValidatedToSeeIfTheyHaveBeenPopulated()
    {
        //Act
        var actual =  _createCommandValidator.Validate(_createAccountCommand);

        //Assert
        Assert.IsTrue(actual.IsValid());
    }

    [Test]
    public void ThenFalseIsReturnedIfThefieldsArentPopulated()
    {
        //Act
        var actual = _createCommandValidator.Validate(new CreateAccountCompleteCommand());

        //Assert
        Assert.IsFalse(actual.IsValid());
    }
}