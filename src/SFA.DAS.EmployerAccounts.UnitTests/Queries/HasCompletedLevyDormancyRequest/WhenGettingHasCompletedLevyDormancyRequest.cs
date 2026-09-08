using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Data;
using SFA.DAS.EmployerAccounts.Models.LevyDormancy;
using SFA.DAS.EmployerAccounts.Queries.HasCompletedLevyDormancyRequest;

namespace SFA.DAS.EmployerAccounts.UnitTests.Queries.HasCompletedLevyDormancyRequest;

[TestFixture]
public class WhenGettingHasCompletedLevyDormancyRequest
{
    [Test]
    public async Task Completed_request_for_account_returns_true()
    {
        var dbContext = CreateDbContext();
        await SeedRequest(dbContext, accountId: 10, LevyDormancyRequestStatus.Completed);
        var handler = CreateHandler(dbContext);

        var result = await handler.Handle(
            new HasCompletedLevyDormancyRequestQuery { AccountId = 10 },
            CancellationToken.None);

        result.HasCompletedRequest.Should().BeTrue();
    }

    [TestCase(LevyDormancyRequestStatus.Pending)]
    [TestCase(LevyDormancyRequestStatus.InProgress)]
    [TestCase(LevyDormancyRequestStatus.Cancelled)]
    public async Task Non_completed_request_for_account_returns_false(LevyDormancyRequestStatus status)
    {
        var dbContext = CreateDbContext();
        await SeedRequest(dbContext, accountId: 10, status);
        var handler = CreateHandler(dbContext);

        var result = await handler.Handle(
            new HasCompletedLevyDormancyRequestQuery { AccountId = 10 },
            CancellationToken.None);

        result.HasCompletedRequest.Should().BeFalse();
    }

    [Test]
    public async Task No_requests_returns_false()
    {
        var dbContext = CreateDbContext();
        var handler = CreateHandler(dbContext);

        var result = await handler.Handle(
            new HasCompletedLevyDormancyRequestQuery { AccountId = 10 },
            CancellationToken.None);

        result.HasCompletedRequest.Should().BeFalse();
    }

    [Test]
    public async Task Completed_request_for_different_account_returns_false()
    {
        var dbContext = CreateDbContext();
        await SeedRequest(dbContext, accountId: 99, LevyDormancyRequestStatus.Completed);
        var handler = CreateHandler(dbContext);

        var result = await handler.Handle(
            new HasCompletedLevyDormancyRequestQuery { AccountId = 10 },
            CancellationToken.None);

        result.HasCompletedRequest.Should().BeFalse();
    }

    private static EmployerAccountsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<EmployerAccountsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new EmployerAccountsDbContext(options);
    }

    private static HasCompletedLevyDormancyRequestQueryHandler CreateHandler(EmployerAccountsDbContext dbContext)
    {
        return new HasCompletedLevyDormancyRequestQueryHandler(
            new Lazy<EmployerAccountsDbContext>(() => dbContext));
    }

    private static async Task SeedRequest(
        EmployerAccountsDbContext dbContext,
        long accountId,
        LevyDormancyRequestStatus status)
    {
        var now = new DateTime(2026, 9, 1);

        dbContext.LevyDormancyRequests.Add(new LevyDormancyRequest
        {
            AccountId = accountId,
            NoLevyDeclaredMonths = 20,
            LastLevyDeclarationDate = now.AddMonths(-24),
            Status = status,
            CreatedOn = now,
            UpdatedOn = now
        });

        await dbContext.SaveChangesAsync();
    }
}
