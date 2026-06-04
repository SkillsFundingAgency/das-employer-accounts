using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Queries.QueryAccounts;
using SFA.DAS.Common.Domain.Types;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.QueryAccounts;

[TestFixture]
public class WhenHandlingQueryAccountsRequest
{
    [Test]
    public async Task ThenAccountsAreReturned()
    {
        // Arrange
        var repository = new Mock<IEmployerAccountRepository>();
        var handler = new QueryAccountsQueryHandler(repository.Object, new QueryAccountsValidator());

        var request = new QueryAccountsRequest
        {
            AccountIds = [1, 2],
            Select = [AccountQueryFields.ApprenticeshipEmployerType],
            Include = [AccountQueryFields.LegalEntities]
        };

        repository.Setup(r => r.GetAccountQuerySummaries(
                It.Is<IReadOnlyList<long>>(ids => ids.SequenceEqual(new long[] { 1, 2 })),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new AccountQuerySummary
                {
                    AccountId = 1,
                    ApprenticeshipEmployerType = ApprenticeshipEmployerType.Levy,
                    LegalEntityIds = [10]
                },
                new AccountQuerySummary
                {
                    AccountId = 2,
                    ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy,
                    LegalEntityIds = [20]
                }
            ]);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Accounts.Should().HaveCount(2);
        result.Accounts[0].ApprenticeshipEmployerType.Should().Be("Levy");
        result.Accounts[0].LegalEntities.Should().ContainSingle(x => x.Id == "10");
    }
}
