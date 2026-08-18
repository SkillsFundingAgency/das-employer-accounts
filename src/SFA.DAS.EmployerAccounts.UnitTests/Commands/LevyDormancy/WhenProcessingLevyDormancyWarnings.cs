using System;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Commands.LevyDormancy;
using SFA.DAS.EmployerAccounts.Commands.SendNotification;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Data;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Models.AccountTeam;
using SFA.DAS.EmployerAccounts.Models.LevyDormancy;
using SFA.DAS.EmployerAccounts.Time;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.LevyDormancy;

[TestFixture]
public class WhenProcessingLevyDormancyWarnings
{
    private const string BaseUrl = "https://localhost:44344";

    [Test]
    public async Task Does_nothing_when_orchestration_is_disabled()
    {
        // Arrange
        var now = new DateTime(2026, 6, 1);
        var dbContext = CreateDbContext();
        await SeedPendingRequest(dbContext, now);
        var handler = CreateHandler(dbContext, new LevyDormancyConfiguration { OrchestrationEnabled = false }, now);

        // Act
        var result = await handler.Handle(new ProcessLevyDormancyWarningsCommand(), CancellationToken.None);

        // Assert
        result.RequestsProcessed.Should().Be(0);
        result.EmailsSent.Should().Be(0);
        (await dbContext.LevyDormancyRequests.SingleAsync()).WarningEmailSentAt.Should().BeNull();
    }

    [Test]
    public async Task Sends_warning_email_and_moves_request_to_in_progress()
    {
        // Arrange
        var now = new DateTime(2026, 6, 1);
        var lastDeclaration = now.AddMonths(-24);
        var dbContext = CreateDbContext();
        await SeedLevyAccount(dbContext, now);
        await SeedPendingRequest(dbContext, now, lastDeclaration);
        var sentCommands = new List<SendNotificationCommand>();
        var configuration = new LevyDormancyConfiguration
        {
            OrchestrationEnabled = true,
            MonthsBetweenInitialWarningAndSwitch = 1
        };
        var handler = CreateHandler(
            dbContext,
            configuration,
            now,
            sentCommands);

        // Act
        var result = await handler.Handle(new ProcessLevyDormancyWarningsCommand(), CancellationToken.None);

        // Assert
        result.RequestsProcessed.Should().Be(1);
        result.EmailsSent.Should().Be(1);
        sentCommands.Should().HaveCount(1);
        sentCommands[0].TemplateId.Should().Be("LevyDormancyInitialWarning");
        sentCommands[0].RecipientsAddress.Should().Be("owner@test.com");
        sentCommands[0].Tokens.Should().ContainKey("switch_date");
        sentCommands[0].Tokens["switch_date"].Should().Be(
            now.AddMonths(configuration.MonthsBetweenInitialWarningAndSwitch).ToString("dd MMM yyyy"));
        sentCommands[0].Tokens["unsubscribe_url"].Should().Be($"{BaseUrl}/settings/notifications");

        var request = await dbContext.LevyDormancyRequests.SingleAsync();
        request.WarningEmailSentAt.Should().Be(now);
        request.Status.Should().Be(LevyDormancyRequestStatus.InProgress);
    }

    [Test]
    public async Task Does_not_resend_when_warning_already_sent()
    {
        // Arrange
        var now = new DateTime(2026, 6, 1);
        var dbContext = CreateDbContext();
        await SeedLevyAccount(dbContext, now);
        dbContext.LevyDormancyRequests.Add(new LevyDormancyRequest
        {
            AccountId = 1,
            NoLevyDeclaredMonths = 20,
            LastLevyDeclarationDate = now.AddMonths(-22),
            Status = LevyDormancyRequestStatus.InProgress,
            CreatedOn = now.AddDays(-7),
            UpdatedOn = now.AddDays(-1),
            WarningEmailSentAt = now.AddDays(-1)
        });
        await dbContext.SaveChangesAsync();
        var sentCommands = new List<SendNotificationCommand>();
        var handler = CreateHandler(
            dbContext,
            new LevyDormancyConfiguration { OrchestrationEnabled = true },
            now,
            sentCommands);

        // Act
        var result = await handler.Handle(new ProcessLevyDormancyWarningsCommand(), CancellationToken.None);

        // Assert
        result.RequestsProcessed.Should().Be(0);
        sentCommands.Should().BeEmpty();
    }

    [Test]
    public async Task Cancels_request_when_account_is_no_longer_levy()
    {
        // Arrange
        var now = new DateTime(2026, 6, 1);
        var dbContext = CreateDbContext();
        await SeedAccount(dbContext, now, ApprenticeshipEmployerType.NonLevy);
        await SeedPendingRequest(dbContext, now);
        var sentCommands = new List<SendNotificationCommand>();
        var handler = CreateHandler(
            dbContext,
            new LevyDormancyConfiguration { OrchestrationEnabled = true },
            now,
            sentCommands);

        // Act
        var result = await handler.Handle(new ProcessLevyDormancyWarningsCommand(), CancellationToken.None);

        // Assert
        result.RequestsCancelled.Should().Be(1);
        sentCommands.Should().BeEmpty();
        (await dbContext.LevyDormancyRequests.SingleAsync()).Status.Should().Be(LevyDormancyRequestStatus.Cancelled);
    }

    [Test]
    public async Task Leaves_request_pending_when_initial_warning_month_not_reached()
    {
        // Arrange
        var now = new DateTime(2026, 6, 1);
        var lastDeclaration = now.AddMonths(-20);
        var dbContext = CreateDbContext();
        await SeedLevyAccount(dbContext, now);
        await SeedPendingRequest(dbContext, now, lastDeclaration);
        var sentCommands = new List<SendNotificationCommand>();
        var handler = CreateHandler(
            dbContext,
            new LevyDormancyConfiguration { OrchestrationEnabled = true },
            now,
            sentCommands);

        // Act
        var result = await handler.Handle(new ProcessLevyDormancyWarningsCommand(), CancellationToken.None);

        // Assert
        result.SkippedNotYetEligible.Should().Be(1);
        sentCommands.Should().BeEmpty();

        var request = await dbContext.LevyDormancyRequests.SingleAsync();
        request.WarningEmailSentAt.Should().BeNull();
        request.Status.Should().Be(LevyDormancyRequestStatus.Pending);
    }

    [Test]
    public async Task Sends_warning_email_to_owner_even_when_notifications_opted_out()
    {
        // Arrange
        var now = new DateTime(2026, 6, 1);
        var dbContext = CreateDbContext();
        await SeedLevyAccount(dbContext, now);
        await SeedPendingRequest(dbContext, now);
        var sentCommands = new List<SendNotificationCommand>();
        var handler = CreateHandler(
            dbContext,
            new LevyDormancyConfiguration { OrchestrationEnabled = true },
            now,
            sentCommands,
            teamMembers:
            [
                new TeamMember
                {
                    AccountId = 1,
                    Email = "owner@test.com",
                    FirstName = "Alex",
                    Role = Role.Owner,
                    Status = InvitationStatus.Accepted,
                    CanReceiveNotifications = false
                }
            ]);

        // Act
        var result = await handler.Handle(new ProcessLevyDormancyWarningsCommand(), CancellationToken.None);

        // Assert
        result.EmailsSent.Should().Be(1);
        sentCommands.Should().HaveCount(1);
        sentCommands[0].RecipientsAddress.Should().Be("owner@test.com");
    }

    [Test]
    public async Task Leaves_request_pending_when_no_recipients_are_available()
    {
        // Arrange
        var now = new DateTime(2026, 6, 1);
        var dbContext = CreateDbContext();
        await SeedLevyAccount(dbContext, now);
        await SeedPendingRequest(dbContext, now);
        var sentCommands = new List<SendNotificationCommand>();
        var handler = CreateHandler(
            dbContext,
            new LevyDormancyConfiguration { OrchestrationEnabled = true },
            now,
            sentCommands,
            teamMembers: []);

        // Act
        var result = await handler.Handle(new ProcessLevyDormancyWarningsCommand(), CancellationToken.None);

        // Assert
        result.SkippedNoRecipients.Should().Be(1);
        sentCommands.Should().BeEmpty();

        var request = await dbContext.LevyDormancyRequests.SingleAsync();
        request.WarningEmailSentAt.Should().BeNull();
        request.Status.Should().Be(LevyDormancyRequestStatus.Pending);
    }

    [Test]
    public async Task Sends_warnings_for_multiple_requests()
    {
        var now = new DateTime(2026, 6, 1);
        var lastDeclaration = now.AddMonths(-24);
        var dbContext = CreateDbContext();
        await SeedAccount(dbContext, now, ApprenticeshipEmployerType.Levy, accountId: 1);
        await SeedAccount(dbContext, now, ApprenticeshipEmployerType.Levy, accountId: 2);
        await SeedPendingRequest(dbContext, now, lastDeclaration, accountId: 1);
        await SeedPendingRequest(dbContext, now, lastDeclaration, accountId: 2);
        var sentCommands = new List<SendNotificationCommand>();
        var handler = CreateHandler(
            dbContext,
            new LevyDormancyConfiguration
            {
                OrchestrationEnabled = true,
                MonthsBetweenInitialWarningAndSwitch = 1
            },
            now,
            sentCommands);

        var result = await handler.Handle(new ProcessLevyDormancyWarningsCommand(), CancellationToken.None);

        result.RequestsProcessed.Should().Be(2);
        result.EmailsSent.Should().Be(2);
        sentCommands.Should().HaveCount(2);
        dbContext.LevyDormancyRequests.Should().OnlyContain(r => r.Status == LevyDormancyRequestStatus.InProgress);
    }

    private static EmployerAccountsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<EmployerAccountsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new EmployerAccountsDbContext(options);
    }

    private static ProcessLevyDormancyWarningsCommandHandler CreateHandler(
        EmployerAccountsDbContext dbContext,
        LevyDormancyConfiguration levyDormancyConfiguration,
        DateTime now,
        List<SendNotificationCommand> sentCommands = null,
        List<TeamMember> teamMembers = null)
    {
        sentCommands ??= [];

        teamMembers ??=
        [
            new TeamMember
            {
                AccountId = 1,
                Email = "owner@test.com",
                FirstName = "Alex",
                Role = Role.Owner,
                Status = InvitationStatus.Accepted,
                CanReceiveNotifications = true
            }
        ];

        var accountTeamRepository = new Mock<IEmployerAccountTeamRepository>();
        accountTeamRepository
            .Setup(r => r.GetAccountTeamMembers(It.IsAny<long>()))
            .ReturnsAsync(teamMembers);

        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<SendNotificationCommand>(), It.IsAny<CancellationToken>()))
            .Callback<SendNotificationCommand, CancellationToken>((command, _) => sentCommands.Add(command))
            .Returns(Task.CompletedTask);

        return new ProcessLevyDormancyWarningsCommandHandler(
            new Lazy<EmployerAccountsDbContext>(() => dbContext),
            mediator.Object,
            accountTeamRepository.Object,
            Options.Create(levyDormancyConfiguration),
            Options.Create(new EmployerAccountsConfiguration { EmployerAccountsBaseUrl = BaseUrl }),
            new CurrentDateTime(now),
            Mock.Of<ILogger<ProcessLevyDormancyWarningsCommandHandler>>());
    }

    private static async Task SeedLevyAccount(EmployerAccountsDbContext dbContext, DateTime assessedOn)
    {
        await SeedAccount(dbContext, assessedOn, ApprenticeshipEmployerType.Levy);
    }

    private static async Task SeedAccount(
        EmployerAccountsDbContext dbContext,
        DateTime assessedOn,
        ApprenticeshipEmployerType employerType,
        long accountId = 1)
    {
        dbContext.Accounts.Add(new Account
        {
            Id = accountId,
            Name = "Test Employer",
            CreatedDate = assessedOn,
            ApprenticeshipEmployerType = (byte)employerType
        });

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedPendingRequest(
        EmployerAccountsDbContext dbContext,
        DateTime createdOn,
        DateTime? lastDeclaration = null,
        long accountId = 1)
    {
        dbContext.LevyDormancyRequests.Add(new LevyDormancyRequest
        {
            AccountId = accountId,
            NoLevyDeclaredMonths = 20,
            LastLevyDeclarationDate = lastDeclaration ?? createdOn.AddMonths(-24),
            Status = LevyDormancyRequestStatus.Pending,
            CreatedOn = createdOn.AddDays(-7),
            UpdatedOn = createdOn.AddDays(-7)
        });

        await dbContext.SaveChangesAsync();
    }
}
