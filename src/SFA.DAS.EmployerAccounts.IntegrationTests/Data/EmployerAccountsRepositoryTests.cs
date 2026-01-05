using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Data;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.Testing.Helpers;

namespace SFA.DAS.EmployerAccounts.IntegrationTests.Data;

[TestFixture]
public class EmployerAccountsRepositoryTests
{
    [Test]
    public Task GetAccountsSinceDate_SinglePage_AllResults()
    {
        var baseDate = new DateTime(2125, 01, 01, 0, 0, 0, DateTimeKind.Local);

        var fixtures = new EmployerAccountsRepositoryTestFixtures()
            .WithSeededAccounts(baseDate, 10);

        return fixtures.CheckEmployerAccountRepository(async repo =>
        {
            var page = await repo.GetAccounts(baseDate, 1, 1000);

            Assert.That(page.AccountsCount, Is.EqualTo(10));
            Assert.That(page.AccountList.Count, Is.EqualTo(10));
        });
    }

    [Test]
    public Task GetAccountsSinceDate_MultiplePages_AllResults()
    {
        var baseDate = new DateTime(2125, 01, 01, 0, 0, 0, DateTimeKind.Local);

        var fixtures = new EmployerAccountsRepositoryTestFixtures()
            .WithSeededAccounts(baseDate, 50);

        int pageSize = 20;

        return fixtures.CheckEmployerAccountRepository(async repo =>
        {
            var pageOne = await repo.GetAccounts(baseDate, 1, pageSize);
            var pageTwo = await repo.GetAccounts(baseDate, 2, pageSize);
            var pageThree = await repo.GetAccounts(baseDate, 3, pageSize);

            Assert.That(pageOne.AccountList.Count, Is.EqualTo(pageSize));
            Assert.That(pageTwo.AccountList.Count, Is.EqualTo(pageSize));
            Assert.That(pageThree.AccountList.Count, Is.EqualTo(10));
            Assert.That(pageOne.AccountsCount, Is.EqualTo(50));

            var idsPageOne = pageOne.AccountList.Select(a => a.Id);
            var idsPageTwo = pageTwo.AccountList.Select(a => a.Id);
            Assert.That(idsPageOne.Intersect(idsPageTwo), Is.Empty);
        });
    }

    [Test]
    public Task GetAccountsSinceDate_ShouldOnlyHaveOnePage_QueryingSecondPage()
    {
        var baseDate = new DateTime(2125, 01, 01, 0, 0, 0, DateTimeKind.Local);

        var fixtures = new EmployerAccountsRepositoryTestFixtures()
            .WithSeededAccounts(baseDate, 5);

        int pageSize = 1000;

        return fixtures.CheckEmployerAccountRepository(async repo =>
        {
            var pageOne = await repo.GetAccounts(baseDate, 1, pageSize);
            var pageTwo = await repo.GetAccounts(baseDate, 2, pageSize);

            Assert.That(pageOne.AccountsCount, Is.EqualTo(5));
            Assert.That(pageOne.AccountList.Count, Is.EqualTo(5));
            Assert.That(pageTwo.AccountsCount, Is.EqualTo(5));
            Assert.That(pageTwo.AccountList.Count, Is.EqualTo(0));
        });
    }

    [Test]
    public Task GetAccountsSinceDate_SinceDateIsFuture_ExpectNoResults()
    {
        var baseDate = new DateTime(2125, 01, 01, 0, 0, 0, DateTimeKind.Local);

        var fixtures = new EmployerAccountsRepositoryTestFixtures()
            .WithSeededAccounts(baseDate, 10);

        return fixtures.CheckEmployerAccountRepository(async repo =>
        {
            var futureDate = baseDate.AddDays(10);
            var page = await repo.GetAccounts(futureDate, 1, 10);

            Assert.That(page.AccountList, Is.Empty);
            Assert.That(page.AccountsCount, Is.EqualTo(0));
        });
    }

    [Test]
    public Task GetAccountsSinceDate_SinceDateProvided_ExpectFilteredResults()
    {
        var baseDate = new DateTime(2125, 01, 01, 0, 0, 0, DateTimeKind.Local);

        var fixtures = new EmployerAccountsRepositoryTestFixtures()
            .WithSeededAccounts(baseDate, 10)
            .WithAdditionalSeededAccounts(baseDate.AddDays(1), 2, "Recently updated");

        return fixtures.CheckEmployerAccountRepository(async repo =>
        {
            var filtered = await repo.GetAccounts(baseDate.AddHours(12), 1, 1000);

            Assert.That(filtered.AccountsCount, Is.EqualTo(2));
            Assert.That(filtered.AccountList.All(a => a.Name.StartsWith("Recently updated")), Is.True);
        });
    }

    [Test]
    public async Task GetAccountsSinceDate_NullSinceDate_IncludesExistingAndSeededAccounts()
    {
        var baseDate = new DateTime(2125, 01, 01, 0, 0, 0, DateTimeKind.Local);
        const int seededCount = 5;

        var fixtures = new EmployerAccountsRepositoryTestFixtures();

        var baselineCount = await fixtures.GetExistingAccountCount();

        fixtures.WithSeededAccounts(baseDate, seededCount);

        await fixtures.CheckEmployerAccountRepository(async repo =>
        {
            var page = await repo.GetAccounts((DateTime?)null, 1, 1000);

            Assert.That(page.AccountsCount, Is.EqualTo(baselineCount + seededCount));
            Assert.That(page.AccountList.Count, Is.EqualTo(
                Math.Min(1000, baselineCount + seededCount)));
        });
    }
}

internal class EmployerAccountsRepositoryTestFixtures
{
    private readonly List<Func<EmployerAccountsDbContext, Task>> _seedActions = new();

    public EmployerAccountsRepositoryTestFixtures()
    {
        EmployerAccountsConfiguration =
            ConfigurationTestHelper.GetConfiguration<EmployerAccountsConfiguration>(
                ConfigurationKeys.EmployerAccounts);
    }

    public EmployerAccountsConfiguration EmployerAccountsConfiguration { get; }

    public EmployerAccountsRepositoryTestFixtures WithSeededAccounts(DateTime baseDate, int count = 10)
    {
        _seedActions.Add(async db =>
        {
            for (int i = 0; i < count; i++)
            {
                db.Accounts.Add(new Account
                {
                    CreatedDate = baseDate.AddMinutes(i),
                    ModifiedDate = baseDate.AddMinutes(i),
                    Name = $"Test Account {i}",
                    HashedId = $"ZZ{i:D4}"
                });
            }

            await db.SaveChangesAsync();
        });

        return this;
    }

    public EmployerAccountsRepositoryTestFixtures WithAdditionalSeededAccounts(DateTime date, int count = 5, string? namePrefix = null)
    {
        _seedActions.Add(async db =>
        {
            var prefix = namePrefix ?? "Extra Account";
            var startIndex = await db.Accounts.CountAsync();

            for (int i = 0; i < count; i++)
            {
                db.Accounts.Add(new Account
                {
                    CreatedDate = date.AddMinutes(i),
                    ModifiedDate = date.AddMinutes(i),
                    Name = $"{prefix} {startIndex + i}",
                    HashedId = $"ZZX{startIndex + i:D4}"
                });
            }

            await db.SaveChangesAsync();
        });

        return this;
    }

    public async Task<int> GetExistingAccountCount()
    {
        await using var db = CreateDbContext();
        return await db.Accounts.CountAsync();
    }


    /// <summary>
    /// Executes all deferred seeding and the repository test action inside one transaction.
    /// Transaction is rolled back after the test completes, ensuring a clean DB.
    /// </summary>
    public Task CheckEmployerAccountRepository(Func<EmployerAccountRepository, Task> testAction)
    {
        return RunWithTransaction(
            repositoryCreator: db => new EmployerAccountRepository(new Lazy<EmployerAccountsDbContext>(() => db)),
            testAction: testAction);
    }

    private async Task RunWithTransaction<TRepository>(
        Func<EmployerAccountsDbContext, TRepository> repositoryCreator,
        Func<TRepository, Task> testAction)
    {
        await using var db = CreateDbContext();
        await db.Database.BeginTransactionAsync();

        try
        {
            foreach (var seed in _seedActions)
            {
                await seed(db);
            }

            var repo = repositoryCreator(db);
            await testAction(repo);

            await db.Database.CurrentTransaction.RollbackAsync();
        }
        catch
        {
            if (db.Database.CurrentTransaction != null)
                await db.Database.CurrentTransaction.RollbackAsync();
            throw;
        }
    }

    public EmployerAccountsDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<EmployerAccountsDbContext>();
        optionsBuilder.UseSqlServer(EmployerAccountsConfiguration.DatabaseConnectionString);
        return new EmployerAccountsDbContext(optionsBuilder.Options);
    }
}
