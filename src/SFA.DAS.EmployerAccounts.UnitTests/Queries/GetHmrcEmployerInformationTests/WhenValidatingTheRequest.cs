﻿using System.Collections.Generic;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Queries.GetHmrcEmployerInformation;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.GetHmrcEmployerInformationTests
{
    public class WhenValidatingTheRequest
    {
        private GetHmrcEmployerInformationValidator _validator;

        [SetUp]
        public void Arrange()
        {
            _validator = new GetHmrcEmployerInformationValidator();            
        }

        [Test]
        public void ThenMessageIsValidWhenAllFieldsArePopulated()
        {
            //Act
            var actual = _validator.Validate(new GetHmrcEmployerInformationQuery {AuthToken = "someValue"});

            //Assert
            Assert.That(actual.IsValid(), Is.True);
        }

        [Test]
        public void ThenTheDictionaryIsPopulatedWithEmptyFields()
        {
            //Act
            var actual = _validator.Validate(new GetHmrcEmployerInformationQuery());

            //Assert
            Assert.That(actual.IsValid(), Is.False);
            Assert.That(actual.ValidationDictionary, Does.Contain(new KeyValuePair<string,string>("AuthToken","AuthToken has not been supplied")));
        }
    }
}
