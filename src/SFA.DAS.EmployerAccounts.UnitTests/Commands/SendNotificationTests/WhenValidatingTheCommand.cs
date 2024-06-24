using System.Collections.Generic;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.SendNotification;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.SendNotificationTests;

public class WhenValidatingTheCommand
{
    private SendNotificationCommandValidator _validator;

    [SetUp]
    public void Arrange()
    {
        _validator = new SendNotificationCommandValidator();
    }

    [Test]
    public void ThenTheCommandIsNotValidIfNoFieldsArePopulated()
    {
        //Act
        var actual = _validator.Validate(new SendNotificationCommand());

        //Assert
        Assert.IsFalse(actual.IsValid());
    }

    [Test]
    public void ThenTheErrorDictionaryIsPopulatedWhenNotValid()
    {
        //Act
        var actual = _validator.Validate(new SendNotificationCommand());

        //Assert
        Assert.Contains(new KeyValuePair<string, string>("RecipientsAddress", "RecipientsAddress has not been supplied"), actual.ValidationDictionary);
        Assert.Contains(new KeyValuePair<string, string>("TemplateId", "TemplateId has not been supplied"), actual.ValidationDictionary);
    }

    [Test]
    public void ThenIsValidWhenAllFieldsHaveBeenSupplied()
    {
        //Assert
        var actual = _validator.Validate(new SendNotificationCommand { RecipientsAddress = "test", TemplateId = "test" });

        //Assert
        Assert.IsTrue(actual.IsValid());
    }
}