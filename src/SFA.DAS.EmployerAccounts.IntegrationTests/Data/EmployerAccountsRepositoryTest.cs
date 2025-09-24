using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
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
    public async Task Constructor_Valid_ShouldNotThrowException()
    {
        var fixtures = new EmployerAccountsRepositoryTestFixtures();

        await fixtures.CheckEmployerAccountRepository(repo => Task.CompletedTask);

        Assert.Pass("Did not get an exception");
    }

    [Test]
    public Task GetAccountUpdates_SinglePage_AllResults()
    {
        // Arrange
        var fixtures = new EmployerAccountsRepositoryTestFixtures();

        return fixtures.CheckEmployerAccountRepository(async repo =>
        {
            var accountUpdates = await repo.GetAllAccountsUpdates(null, 1, 1000);
            Assert.That(accountUpdates.Accounts.AccountsCount == accountUpdates.Accounts.AccountList.Count());
        });

    }

    [Test]
    public Task GetAccountUpdates_MultiplePages_AllResults()
    {
        // Arrange
        var fixtures = new EmployerAccountsRepositoryTestFixtures();
        int pageSize = 20;

        return fixtures.CheckEmployerAccountRepository(async repo =>
        {
            var pageOne = await repo.GetAllAccountsUpdates(null, 1, pageSize);
            Assert.That(pageOne.Accounts.AccountsCount != pageOne.Accounts.AccountList.Count());
            Assert.That(pageOne.Accounts.AccountList.Count <= pageSize);

            var pageTwo = await repo.GetAllAccountsUpdates(null, 2, pageSize);
            Assert.That(pageTwo.Accounts.AccountsCount != pageTwo.Accounts.AccountList.Count());
            Assert.That(pageTwo.Accounts.AccountList.Count <= pageSize);

            foreach(var account in pageOne.Accounts.AccountList)
            {
                Assert.That(!pageTwo.Accounts.AccountList.Contains(account));
            }
        });

    }


    [Test]
    public Task GetAccountUpdates_ShouldOnlyHaveOnePage_QueryingSecondPage()
    {
        // Arrange
        var fixtures = new EmployerAccountsRepositoryTestFixtures();
        int pageSize = 1000;

        return fixtures.CheckEmployerAccountRepository(async repo =>
        {
            var pageOne = await repo.GetAllAccountsUpdates(null, 1, pageSize);
            Assert.That(pageOne.Accounts.AccountsCount == pageOne.Accounts.AccountList.Count());
            Assert.That(pageOne.Accounts.AccountList.Count <= pageSize);

            var pageTwo = await repo.GetAllAccountsUpdates(null, 2, pageSize);
            Assert.That(pageTwo.Accounts.AccountsCount == 0);
            Assert.That(pageTwo.Accounts.AccountList.Count == 0);
        });

    }

    [Test]
    public Task GetAccountUpdates_SinceDateIsFuture_ExpectNoResults()
    {
        // Arrange
        var fixtures = new EmployerAccountsRepositoryTestFixtures();
        int pageSize = 1000;

        return fixtures.CheckEmployerAccountRepository(async repo =>
        {
            var pageOne = await repo.GetAllAccountsUpdates(DateTime.MaxValue, 1, pageSize);
            Assert.That(pageOne.Accounts.AccountsCount == 0);
            Assert.That(pageOne.Accounts.AccountList.Count == 0);
        });

    }

    [Test]
    public Task GetAccountUpdates_SinceDateProvided_ExpectFilteredResults()
    {
        // Arrange
        var fixtures = new EmployerAccountsRepositoryTestFixtures();
        int pageSize = 1000;

        return fixtures.CheckEmployerAccountRepository(async repo =>
        {
            var dateProvided = await repo.GetAllAccountsUpdates(DateTime.Now, 1, pageSize);
            var noDateProvided = await repo.GetAllAccountsUpdates(DateTime.MinValue, 1, pageSize);

            Assert.That(dateProvided.Accounts.AccountsCount != noDateProvided.Accounts.AccountsCount);
            Assert.That(dateProvided.Accounts.AccountList.Count != noDateProvided.Accounts.AccountList.Count);
        });

    }
}

internal class EmployerAccountsRepositoryTestFixtures
{
    public EmployerAccountsRepositoryTestFixtures()
    {
        EmployerAccountsConfiguration = ConfigurationTestHelper.GetConfiguration<EmployerAccountsConfiguration>(ConfigurationKeys.EmployerAccounts);
        EmployerAccountRepositoryLoggerMock = new Mock<ILogger<EmployerAccountRepository>>();
    }

    public EmployerAccountsConfiguration EmployerAccountsConfiguration { get; }

    public Mock<ILogger<EmployerAccountRepository>> EmployerAccountRepositoryLoggerMock { get; private set; }
    public ILogger<EmployerAccountRepository> EmployerAccountRepositoryLogger => EmployerAccountRepositoryLoggerMock.Object;

    public Task CheckEmployerAccountRepository(Func<EmployerAccountRepository, Task> action)
    {
        return RunWithTransaction(
            repositoryCreator: db => new EmployerAccountRepository(new Lazy<EmployerAccountsDbContext>(() => db)),
            action: action);
    }

    private async Task RunWithTransaction<TRepository>(
        Func<EmployerAccountsDbContext, TRepository> repositoryCreator,
        Func<TRepository, Task> action)
    {
        await using var db = CreateDbContext();
        await db.Database.BeginTransactionAsync();

        try
        {
            var repo = repositoryCreator(db);
            await action(repo);

            await db.Database.CurrentTransaction.CommitAsync();
        }
        catch (Exception)
        {
            await db.Database.CurrentTransaction.RollbackAsync();
            throw;
        }
    }

    private static Account CreateAccount(EmployerAccountsDbContext db)
    {
        var hashedId = GetNextHashedIdForTests(db);
        var account = new Account
        {
            CreatedDate = DateTime.Now,
            HashedId = hashedId,
            Name = $"Account created for unit test {hashedId}"
        };
        db.Accounts.Add(account);

        return account;
    }

    private static string GetNextHashedIdForTests(EmployerAccountsDbContext dbContext)
    {
        var regex = new Regex("ZZ[0-9]{4}");

        var maxHashedId = dbContext.Accounts
            .Where(ac => ac.HashedId.StartsWith("ZZ"))
            .AsEnumerable()
            .Where(ac => regex.IsMatch(ac.HashedId))
            .Max(ac => ac.HashedId);

        if (string.IsNullOrWhiteSpace(maxHashedId))
        {
            return "ZZ0001";
        }

        var intPart = int.Parse(maxHashedId.Substring(2, 4)) + 1;
        return $"ZZ{intPart:D4}";
    }

    private EmployerAccountsDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<EmployerAccountsDbContext>();
        optionsBuilder.UseSqlServer(EmployerAccountsConfiguration.DatabaseConnectionString);
        return new EmployerAccountsDbContext(optionsBuilder.Options);
    }
}