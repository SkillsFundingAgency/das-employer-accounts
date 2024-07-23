using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Queries.GetPayeSchemeByRef;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetPayeSchemeByRefTests
{
    public class WhenIValidateTheQuery
    {
        private GetPayeSchemeByRefValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new GetPayeSchemeByRefValidator();
        }

        [Test]
        public void ThenFalseIsReturnedWhenTheAccountIdIsntPopulated()
        {
            //Act
            var actual = _validator.Validate(new GetPayeSchemeByRefQuery { Ref = "ABC/123" });

            //Assert
            actual.Should().NotBeNull();
            actual.IsValid().Should().BeFalse();
            actual.ValidationDictionary
                .Should().ContainKey("AccountId")
                .WhoseValue.Should().Be("AccountId has not been supplied");
        }

        [Test]
        public void ThenFalseIsReturnedWhenTheRefIsntPopulated()
        {
            //Act
            var actual = _validator.Validate(new GetPayeSchemeByRefQuery { AccountId = 12322 });

            //Assert
            actual.Should().NotBeNull();
            actual.IsValid().Should().BeFalse();
            actual.ValidationDictionary
                .Should().ContainKey("Ref")
                .WhoseValue.Should().Be("PayeSchemeRef has not been supplied");
        }

        [Test]
        public void ThenTrueIsReturnedWhenTheIdHasBeenPopulated()
        {
            //Act
            var actual = _validator.Validate(new GetPayeSchemeByRefQuery
            {
                AccountId = 34325,
                Ref = "ABC/123"
            });

            //Assert
            actual.IsValid().Should().BeTrue();
        }

    }
}
