using System;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Commands.LevyDormancy;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Data;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Models.LevyDormancy;
using SFA.DAS.EmployerAccounts.Models.PAYE;
using SFA.DAS.EmployerAccounts.Time;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.LevyDormancy;

[TestFixture]
public class WhenAssessingLevyDormancy
{
    private const string PayeRef = "123/A1";

    [Test]
    public async Task Dormant_levy_account_creates_a_dormancy_request()
    {
        // Arrange
        var assessedOn = new DateTime(2026, 6, 1);
        var lastDeclaration = assessedOn.AddMonths(-22);
        var dbContext = CreateDbContext();
        await SeedLevyAccount(dbContext, assessedOn, lastDeclaration);
        var handler = CreateHandler(dbContext, new LevyDormancyConfiguration { AssessmentEnabled = true }, assessedOn);

        // Act
        var result = await handler.Handle(new AssessLevyDormancyCommand(), CancellationToken.None);

        // Assert
        result.AccountsAssessed.Should().Be(1);
        result.DormantCandidatesFound.Should().Be(1);
        result.DormancyRequestsCreated.Should().Be(1);

        var request = dbContext.LevyDormancyRequests.Single();
        request.AccountId.Should().Be(1);
        request.LastLevyDeclarationDate.Should().Be(lastDeclaration);
        request.Status.Should().Be(LevyDormancyRequestStatus.Pending);
    }

    [Test]
    public async Task Assessment_does_nothing_when_feature_toggle_is_off()
    {
        // Arrange
        var assessedOn = new DateTime(2026, 6, 1);
        var dbContext = CreateDbContext();
        await SeedLevyAccount(dbContext, assessedOn, assessedOn.AddMonths(-30));
        var handler = CreateHandler(dbContext, new LevyDormancyConfiguration { AssessmentEnabled = false }, assessedOn);

        // Act
        var result = await handler.Handle(new AssessLevyDormancyCommand(), CancellationToken.None);

        // Assert
        result.AccountsAssessed.Should().Be(0);
        result.DormancyRequestsCreated.Should().Be(0);
        dbContext.LevyDormancyRequests.Should().BeEmpty();
    }

    [Test]
    public async Task Non_levy_accounts_are_not_candidates()
    {
        // Arrange
        var assessedOn = new DateTime(2026, 6, 1);
        var dbContext = CreateDbContext();
        await SeedAccount(dbContext, assessedOn, ApprenticeshipEmployerType.NonLevy);
        var handler = CreateHandler(dbContext, new LevyDormancyConfiguration { AssessmentEnabled = true }, assessedOn);

        // Act
        var result = await handler.Handle(new AssessLevyDormancyCommand(), CancellationToken.None);

        // Assert
        result.DormantCandidatesFound.Should().Be(0);
        result.DormancyRequestsCreated.Should().Be(0);
    }

    [Test]
    public async Task Recent_declaration_is_not_a_candidate()
    {
        // Arrange
        var assessedOn = new DateTime(2026, 6, 1);
        var lastDeclaration = assessedOn.AddMonths(-6);
        var dbContext = CreateDbContext();
        await SeedLevyAccount(dbContext, assessedOn, lastDeclaration);
        var handler = CreateHandler(dbContext, new LevyDormancyConfiguration { AssessmentEnabled = true }, assessedOn);

        // Act
        var result = await handler.Handle(new AssessLevyDormancyCommand(), CancellationToken.None);

        // Assert
        result.DormantCandidatesFound.Should().Be(0);
        result.DormancyRequestsCreated.Should().Be(0);
    }

    [Test]
    public async Task Declaration_within_detection_window_is_not_a_candidate()
    {
        // Arrange
        var assessedOn = new DateTime(2026, 6, 1);
        var lastDeclaration = assessedOn.AddMonths(-19);
        var dbContext = CreateDbContext();
        await SeedLevyAccount(dbContext, assessedOn, lastDeclaration);
        var handler = CreateHandler(dbContext, new LevyDormancyConfiguration { AssessmentEnabled = true }, assessedOn);

        // Act
        var result = await handler.Handle(new AssessLevyDormancyCommand(), CancellationToken.None);

        // Assert
        result.DormantCandidatesFound.Should().Be(0);
        result.DormancyRequestsCreated.Should().Be(0);
    }

    [Test]
    public async Task Levy_account_without_projection_rows_is_skipped()
    {
        // Arrange
        var assessedOn = new DateTime(2026, 6, 1);
        var dbContext = CreateDbContext();
        await SeedAccount(dbContext, assessedOn, ApprenticeshipEmployerType.Levy);
        var handler = CreateHandler(dbContext, new LevyDormancyConfiguration { AssessmentEnabled = true }, assessedOn);

        // Act
        var result = await handler.Handle(new AssessLevyDormancyCommand(), CancellationToken.None);

        // Assert
        result.DormantCandidatesFound.Should().Be(0);
        result.DormancyRequestsCreated.Should().Be(0);
    }

    [Test]
    public async Task Account_with_pending_request_is_not_retriggered()
    {
        // Arrange
        var assessedOn = new DateTime(2026, 6, 1);
        var lastDeclaration = assessedOn.AddMonths(-30);
        var dbContext = CreateDbContext();
        await SeedLevyAccount(dbContext, assessedOn, lastDeclaration);
        await SeedDormancyRequest(dbContext, LevyDormancyRequestStatus.Pending, assessedOn.AddDays(-1));
        var handler = CreateHandler(dbContext, new LevyDormancyConfiguration { AssessmentEnabled = true }, assessedOn);

        // Act
        var result = await handler.Handle(new AssessLevyDormancyCommand(), CancellationToken.None);

        // Assert
        result.DormantCandidatesFound.Should().Be(0);
        result.DormancyRequestsCreated.Should().Be(0);
        dbContext.LevyDormancyRequests.Count().Should().Be(1);
    }

    [Test]
    public async Task Account_with_in_progress_request_is_not_retriggered()
    {
        // Arrange
        var assessedOn = new DateTime(2026, 6, 1);
        var lastDeclaration = assessedOn.AddMonths(-30);
        var dbContext = CreateDbContext();
        await SeedLevyAccount(dbContext, assessedOn, lastDeclaration);
        await SeedDormancyRequest(dbContext, LevyDormancyRequestStatus.InProgress, assessedOn.AddDays(-1));
        var handler = CreateHandler(dbContext, new LevyDormancyConfiguration { AssessmentEnabled = true }, assessedOn);

        // Act
        var result = await handler.Handle(new AssessLevyDormancyCommand(), CancellationToken.None);

        // Assert
        result.DormantCandidatesFound.Should().Be(0);
        result.DormancyRequestsCreated.Should().Be(0);
    }

    [Test]
    public async Task Removed_paye_with_dormant_declaration_is_assessed()
    {
        // Arrange
        var assessedOn = new DateTime(2026, 6, 1);
        var lastDeclaration = assessedOn.AddMonths(-22);
        var dbContext = CreateDbContext();
        await SeedLevyAccount(dbContext, assessedOn, lastDeclaration, removedDate: assessedOn.AddMonths(-1));
        var handler = CreateHandler(dbContext, new LevyDormancyConfiguration { AssessmentEnabled = true }, assessedOn);

        // Act
        var result = await handler.Handle(new AssessLevyDormancyCommand(), CancellationToken.None);

        // Assert
        result.DormantCandidatesFound.Should().Be(1);
        result.DormancyRequestsCreated.Should().Be(1);
    }

    [Test]
    public async Task Aorn_paye_with_dormant_declaration_is_assessed()
    {
        // Arrange
        var assessedOn = new DateTime(2026, 6, 1);
        var lastDeclaration = assessedOn.AddMonths(-22);
        var dbContext = CreateDbContext();
        await SeedLevyAccount(dbContext, assessedOn, lastDeclaration, aorn: "1234567890ABC");
        var handler = CreateHandler(dbContext, new LevyDormancyConfiguration { AssessmentEnabled = true }, assessedOn);

        // Act
        var result = await handler.Handle(new AssessLevyDormancyCommand(), CancellationToken.None);

        // Assert
        result.DormantCandidatesFound.Should().Be(1);
        result.DormancyRequestsCreated.Should().Be(1);
    }

    [Test]
    public async Task Ignored_account_does_not_create_a_dormancy_request()
    {
        // Arrange
        var assessedOn = new DateTime(2026, 6, 1);
        var lastDeclaration = assessedOn.AddMonths(-22);
        var dbContext = CreateDbContext();
        await SeedLevyAccount(dbContext, assessedOn, lastDeclaration);
        var handler = CreateHandler(
            dbContext,
            new LevyDormancyConfiguration
            {
                AssessmentEnabled = true,
                IgnoredAccountIds = "1"
            },
            assessedOn);

        // Act
        var result = await handler.Handle(new AssessLevyDormancyCommand(), CancellationToken.None);

        // Assert
        result.DormantCandidatesFound.Should().Be(0);
        result.DormancyRequestsCreated.Should().Be(0);
        result.DormancyRequestsSkippedIgnored.Should().Be(0);
        dbContext.LevyDormancyRequests.Should().BeEmpty();
    }

    [Test]
    public async Task Non_ignored_account_still_creates_a_dormancy_request_when_ignore_list_has_other_ids()
    {
        // Arrange
        var assessedOn = new DateTime(2026, 6, 1);
        var lastDeclaration = assessedOn.AddMonths(-22);
        var dbContext = CreateDbContext();
        await SeedLevyAccount(dbContext, assessedOn, lastDeclaration);
        var handler = CreateHandler(
            dbContext,
            new LevyDormancyConfiguration
            {
                AssessmentEnabled = true,
                IgnoredAccountIds = "999,1000"
            },
            assessedOn);

        // Act
        var result = await handler.Handle(new AssessLevyDormancyCommand(), CancellationToken.None);

        // Assert
        result.DormancyRequestsCreated.Should().Be(1);
        result.DormancyRequestsSkippedIgnored.Should().Be(0);
        dbContext.LevyDormancyRequests.Should().HaveCount(1);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task Empty_or_whitespace_ignore_list_behaves_as_today(string ignoredAccountIds)
    {
        // Arrange
        var assessedOn = new DateTime(2026, 6, 1);
        var lastDeclaration = assessedOn.AddMonths(-22);
        var dbContext = CreateDbContext();
        await SeedLevyAccount(dbContext, assessedOn, lastDeclaration);
        var handler = CreateHandler(
            dbContext,
            new LevyDormancyConfiguration
            {
                AssessmentEnabled = true,
                IgnoredAccountIds = ignoredAccountIds
            },
            assessedOn);

        // Act
        var result = await handler.Handle(new AssessLevyDormancyCommand(), CancellationToken.None);

        // Assert
        result.DormancyRequestsCreated.Should().Be(1);
        result.DormancyRequestsSkippedIgnored.Should().Be(0);
    }

    [Test]
    public async Task Accounts_with_active_requests_are_excluded_and_other_dormant_accounts_are_created()
    {
        var assessedOn = new DateTime(2026, 6, 1);
        var lastDeclaration = assessedOn.AddMonths(-22);
        var dbContext = CreateDbContext();

        await SeedLevyAccount(dbContext, assessedOn, lastDeclaration, accountId: 1, payeRef: "123/A1");
        await SeedLevyAccount(dbContext, assessedOn, lastDeclaration, accountId: 2, payeRef: "123/A2");
        await SeedLevyAccount(dbContext, assessedOn, lastDeclaration, accountId: 3, payeRef: "123/A3");
        await SeedDormancyRequest(dbContext, LevyDormancyRequestStatus.Pending, assessedOn.AddDays(-1), accountId: 1);
        await SeedDormancyRequest(dbContext, LevyDormancyRequestStatus.InProgress, assessedOn.AddDays(-1), accountId: 2);

        var handler = CreateHandler(dbContext, new LevyDormancyConfiguration { AssessmentEnabled = true }, assessedOn);

        var result = await handler.Handle(new AssessLevyDormancyCommand(), CancellationToken.None);

        result.AccountsAssessed.Should().Be(3);
        result.DormantCandidatesFound.Should().Be(1);
        result.DormancyRequestsCreated.Should().Be(1);
        dbContext.LevyDormancyRequests.Should().HaveCount(3);
        dbContext.LevyDormancyRequests.Count(r => r.AccountId == 3 && r.Status == LevyDormancyRequestStatus.Pending).Should().Be(1);
    }

    [Test]
    public async Task Multiple_dormant_accounts_are_persisted_in_one_assessment_run()
    {
        var assessedOn = new DateTime(2026, 6, 1);
        var lastDeclaration = assessedOn.AddMonths(-22);
        var dbContext = CreateDbContext();

        await SeedLevyAccount(dbContext, assessedOn, lastDeclaration, accountId: 1, payeRef: "123/A1");
        await SeedLevyAccount(dbContext, assessedOn, lastDeclaration, accountId: 2, payeRef: "123/A2");
        await SeedLevyAccount(dbContext, assessedOn, lastDeclaration, accountId: 3, payeRef: "123/A3");

        var handler = CreateHandler(dbContext, new LevyDormancyConfiguration { AssessmentEnabled = true }, assessedOn);

        var result = await handler.Handle(new AssessLevyDormancyCommand(), CancellationToken.None);

        result.DormantCandidatesFound.Should().Be(3);
        result.DormancyRequestsCreated.Should().Be(3);
        dbContext.LevyDormancyRequests.Should().HaveCount(3);
        dbContext.LevyDormancyRequests.Select(r => r.AccountId).Should().BeEquivalentTo([1L, 2L, 3L]);
    }

    [Test]
    public async Task Malformed_ignore_list_tokens_are_skipped_and_valid_ids_still_apply()
    {
        // Arrange
        var assessedOn = new DateTime(2026, 6, 1);
        var lastDeclaration = assessedOn.AddMonths(-22);
        var dbContext = CreateDbContext();
        await SeedLevyAccount(dbContext, assessedOn, lastDeclaration);
        var handler = CreateHandler(
            dbContext,
            new LevyDormancyConfiguration
            {
                AssessmentEnabled = true,
                IgnoredAccountIds = "abc,1, "
            },
            assessedOn);

        // Act
        var result = await handler.Handle(new AssessLevyDormancyCommand(), CancellationToken.None);

        // Assert
        result.DormancyRequestsCreated.Should().Be(0);
        result.DormancyRequestsSkippedIgnored.Should().Be(0);
        dbContext.LevyDormancyRequests.Should().BeEmpty();
    }

    private static EmployerAccountsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<EmployerAccountsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new EmployerAccountsDbContext(options);
    }

    private static AssessLevyDormancyCommandHandler CreateHandler(
        EmployerAccountsDbContext dbContext,
        LevyDormancyConfiguration configuration,
        DateTime assessedOn)
    {
        return new AssessLevyDormancyCommandHandler(
            new Lazy<EmployerAccountsDbContext>(() => dbContext),
            Options.Create(configuration),
            new CurrentDateTime(assessedOn),
            Mock.Of<ILogger<AssessLevyDormancyCommandHandler>>());
    }

    private static async Task SeedLevyAccount(
        EmployerAccountsDbContext dbContext,
        DateTime assessedOn,
        DateTime? lastDeclaration,
        DateTime? removedDate = null,
        string aorn = null,
        long accountId = 1,
        string payeRef = null)
    {
        await SeedAccount(dbContext, assessedOn, ApprenticeshipEmployerType.Levy, removedDate, aorn, accountId, payeRef);

        if (lastDeclaration.HasValue)
        {
            dbContext.EmployerAccountLevyStatuses.Add(new EmployerAccountLevyStatus
            {
                AccountId = accountId,
                LastLevyDeclarationDate = lastDeclaration,
                LastRefreshedAt = lastDeclaration.Value
            });

            await dbContext.SaveChangesAsync();
        }
    }

    private static async Task SeedAccount(
        EmployerAccountsDbContext dbContext,
        DateTime assessedOn,
        ApprenticeshipEmployerType employerType,
        DateTime? removedDate = null,
        string aorn = null,
        long accountId = 1,
        string payeRef = null)
    {
        payeRef ??= $"{PayeRef}-{accountId}";

        dbContext.Accounts.Add(new Account
        {
            Id = accountId,
            Name = "Test",
            CreatedDate = assessedOn,
            ApprenticeshipEmployerType = (byte)employerType
        });
        dbContext.Payees.Add(new Paye { EmpRef = payeRef, Aorn = aorn });
        dbContext.AccountHistory.Add(new AccountHistory
        {
            AccountId = accountId,
            PayeRef = payeRef,
            AddedDate = assessedOn.AddYears(-3),
            RemovedDate = removedDate
        });

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedDormancyRequest(
        EmployerAccountsDbContext dbContext,
        LevyDormancyRequestStatus status,
        DateTime createdOn,
        long accountId = 1)
    {
        dbContext.LevyDormancyRequests.Add(new LevyDormancyRequest
        {
            AccountId = accountId,
            NoLevyDeclaredMonths = 20,
            LastLevyDeclarationDate = createdOn.AddMonths(-21),
            Status = status,
            CreatedOn = createdOn,
            UpdatedOn = createdOn
        });

        await dbContext.SaveChangesAsync();
    }
}
