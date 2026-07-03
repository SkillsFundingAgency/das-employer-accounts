using System;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Commands.LevyDormancy;
using SFA.DAS.EmployerAccounts.Data;
using SFA.DAS.EmployerAccounts.Models.LevyDormancy;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.LevyDormancy;

[TestFixture]
public class WhenUpsertingEmployerAccountLevyStatus
{
    [Test]
    public async Task Creates_projection_row_when_none_exists()
    {
        // Arrange
        var refreshedAt = new DateTime(2026, 6, 1);
        var lastDeclaration = new DateTime(2024, 3, 15);
        var dbContext = CreateDbContext();
        var handler = CreateHandler(dbContext);

        // Act
        await handler.Handle(new UpsertEmployerAccountLevyStatusCommand
        {
            AccountId = 1,
            LastLevyDeclarationDate = lastDeclaration,
            RefreshedAt = refreshedAt
        }, CancellationToken.None);

        // Assert
        var status = await dbContext.EmployerAccountLevyStatuses.SingleAsync();
        status.AccountId.Should().Be(1);
        status.LastLevyDeclarationDate.Should().Be(lastDeclaration);
        status.LastRefreshedAt.Should().Be(refreshedAt);
    }

    [Test]
    public async Task Updates_existing_projection_row_when_event_is_newer()
    {
        // Arrange
        var refreshedAt = new DateTime(2026, 6, 1);
        var updatedDeclaration = new DateTime(2026, 6, 1);
        var dbContext = CreateDbContext();
        dbContext.EmployerAccountLevyStatuses.Add(new EmployerAccountLevyStatus
        {
            AccountId = 1,
            LastLevyDeclarationDate = new DateTime(2024, 1, 1),
            LastRefreshedAt = refreshedAt.AddDays(-1)
        });
        await dbContext.SaveChangesAsync();
        var handler = CreateHandler(dbContext);

        // Act
        await handler.Handle(new UpsertEmployerAccountLevyStatusCommand
        {
            AccountId = 1,
            LastLevyDeclarationDate = updatedDeclaration,
            RefreshedAt = refreshedAt
        }, CancellationToken.None);

        // Assert
        var status = await dbContext.EmployerAccountLevyStatuses.SingleAsync();
        status.LastLevyDeclarationDate.Should().Be(updatedDeclaration);
        status.LastRefreshedAt.Should().Be(refreshedAt);
    }

    private static EmployerAccountsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<EmployerAccountsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new EmployerAccountsDbContext(options);
    }

    private static UpsertEmployerAccountLevyStatusCommandHandler CreateHandler(EmployerAccountsDbContext dbContext)
    {
        return new UpsertEmployerAccountLevyStatusCommandHandler(
            new Lazy<EmployerAccountsDbContext>(() => dbContext),
            Mock.Of<ILogger<UpsertEmployerAccountLevyStatusCommandHandler>>());
    }
}
