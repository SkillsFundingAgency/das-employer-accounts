using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Queries.QueryAccounts;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.QueryAccounts;

[TestFixture]
public class WhenValidatingQueryAccountsRequest
{
    [Test, MoqAutoData]
    public void ThenRequestIsInvalidWhenNoAccountIds(QueryAccountsValidator validator)
    {
        // Arrange
        var request = new QueryAccountsRequest();

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid().Should().BeFalse();
    }

    [Test, MoqAutoData]
    public void ThenRequestIsInvalidWhenTooManyAccountIds(QueryAccountsValidator validator)
    {
        // Arrange
        var request = new QueryAccountsRequest
        {
            AccountIds = Enumerable.Range(1, QueryAccountsRequest.MaxAccountIds + 1).Select(i => (long)i).ToList()
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid().Should().BeFalse();
    }

    [Test, MoqAutoData]
    public void ThenRequestIsInvalidForUnsupportedSelectField(QueryAccountsValidator validator)
    {
        // Arrange
        var request = new QueryAccountsRequest
        {
            AccountIds = [1],
            Select = ["ownerEmail"]
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid().Should().BeFalse();
    }
}
