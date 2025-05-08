using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Queries.SearchEmployerAccountsByName;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.SearchEmployerAccountsByName;

public class WhenIValidateTheQuery
{
    private SearchEmployerAccountsByNameQueryValidator _validator;

    [SetUp]
    public void Arrange()
    {
        _validator = new SearchEmployerAccountsByNameQueryValidator();
    }

    [Test]
    public void ThenShouldReturnValidIfRequestIsValid()
    {
        //Act
        var result = _validator.Validate(new SearchEmployerAccountsByNameQuery { EmployerName = "Test Employer" });

        //Assert
        result.IsValid().Should().BeTrue();
    }

    [Test]
    public void ThenShouldReturnInvalidIfEmployerNameIsEmpty()
    {
        //Act
        var result = _validator.Validate(new SearchEmployerAccountsByNameQuery { EmployerName = string.Empty });

        //Assert
        result.IsValid().Should().BeFalse();
        result.ValidationDictionary.Should().Contain(new KeyValuePair<string, string>("EmployerName", "Employer name has not been supplied"));
    }

    [Test]
    public void ThenShouldReturnInvalidIfEmployerNameIsNull()
    {
        //Act
        var result = _validator.Validate(new SearchEmployerAccountsByNameQuery { EmployerName = null });

        //Assert
        result.IsValid().Should().BeFalse();
        result.ValidationDictionary.Should().Contain(new KeyValuePair<string, string>("EmployerName", "Employer name has not been supplied"));
    }
} 