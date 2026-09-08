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
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Models.AccountTeam;
using SFA.DAS.EmployerAccounts.Models.LevyDormancy;
using SFA.DAS.EmployerAccounts.Time;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerAccounts.UnitTests.Commands.LevyDormancy;

[TestFixture]
public class WhenProcessingLevyDormancySwitches
{
    [Test]
    public async Task Does_nothing_when_orchestration_is_disabled()
    {
        // Arrange
        var now = new DateTime(2026, 9, 1);
        var dbContext = CreateDbContext();
        await SeedLevyAccount(dbContext, now);
        await SeedInProgressRequest(dbContext, now, warningSentAt: now.AddMonths(-1));
        var accountRepository = new Mock<IEmployerAccountRepository>();
        var eventPublisher = new Mock<IEventPublisher>();
        var sentCommands = new List<SendNotificationCommand>();
        var handler = CreateHandler(
            dbContext,
            new LevyDormancyConfiguration { OrchestrationEnabled = false },
            now,
            accountRepository.Object,
            eventPublisher.Object,
            sentCommands);

        // Act
        var result = await handler.Handle(new ProcessLevyDormancySwitchesCommand(), CancellationToken.None);

        // Assert
        result.RequestsProcessed.Should().Be(0);
        result.AccountsSwitched.Should().Be(0);
        accountRepository.Verify(
            r => r.SetAccountLevyStatus(It.IsAny<long>(), It.IsAny<ApprenticeshipEmployerType>()),
            Times.Never);
        eventPublisher.Verify(p => p.Publish(It.IsAny<ApprenticeshipEmployerTypeChangeEvent>()), Times.Never);
        sentCommands.Should().BeEmpty();
    }

    [Test]
    public async Task Skips_when_months_since_warning_not_reached()
    {
        // Arrange
        var now = new DateTime(2026, 9, 1);
        var dbContext = CreateDbContext();
        await SeedLevyAccount(dbContext, now);
        await SeedInProgressRequest(dbContext, now, warningSentAt: now);
        var accountRepository = new Mock<IEmployerAccountRepository>();
        var eventPublisher = new Mock<IEventPublisher>();
        var sentCommands = new List<SendNotificationCommand>();
        var handler = CreateHandler(
            dbContext,
            new LevyDormancyConfiguration
            {
                OrchestrationEnabled = true,
                MonthsBetweenInitialWarningAndSwitch = 1
            },
            now,
            accountRepository.Object,
            eventPublisher.Object,
            sentCommands);

        // Act
        var result = await handler.Handle(new ProcessLevyDormancySwitchesCommand(), CancellationToken.None);

        // Assert
        result.SkippedNotYetEligible.Should().Be(1);
        result.AccountsSwitched.Should().Be(0);
        accountRepository.Verify(
            r => r.SetAccountLevyStatus(It.IsAny<long>(), It.IsAny<ApprenticeshipEmployerType>()),
            Times.Never);
        sentCommands.Should().BeEmpty();
        (await dbContext.LevyDormancyRequests.SingleAsync()).Status.Should().Be(LevyDormancyRequestStatus.InProgress);
    }

    [Test]
    public async Task Switches_account_publishes_event_and_sends_confirmation_email()
    {
        // Arrange
        var now = new DateTime(2026, 9, 1);
        var dbContext = CreateDbContext();
        await SeedLevyAccount(dbContext, now);
        await SeedInProgressRequest(dbContext, now, warningSentAt: now.AddMonths(-1));
        var accountRepository = new Mock<IEmployerAccountRepository>();
        var eventPublisher = new Mock<IEventPublisher>();
        ApprenticeshipEmployerTypeChangeEvent publishedEvent = null;
        eventPublisher
            .Setup(p => p.Publish(It.IsAny<ApprenticeshipEmployerTypeChangeEvent>()))
            .Callback<ApprenticeshipEmployerTypeChangeEvent>(e => publishedEvent = e)
            .Returns(Task.CompletedTask);
        var sentCommands = new List<SendNotificationCommand>();
        var handler = CreateHandler(
            dbContext,
            new LevyDormancyConfiguration
            {
                OrchestrationEnabled = true,
                MonthsBetweenInitialWarningAndSwitch = 1
            },
            now,
            accountRepository.Object,
            eventPublisher.Object,
            sentCommands);

        // Act
        var result = await handler.Handle(new ProcessLevyDormancySwitchesCommand(), CancellationToken.None);

        // Assert
        result.RequestsProcessed.Should().Be(1);
        result.AccountsSwitched.Should().Be(1);
        result.EmailsSent.Should().Be(1);

        accountRepository.Verify(
            r => r.SetAccountLevyStatus(1, ApprenticeshipEmployerType.NonLevy),
            Times.Once);

        publishedEvent.Should().NotBeNull();
        publishedEvent.AccountId.Should().Be(1);
        publishedEvent.ApprenticeshipEmployerType.Should().Be(ApprenticeshipEmployerType.NonLevy);
        publishedEvent.Created.Should().Be(now);

        sentCommands.Should().HaveCount(1);
        sentCommands[0].TemplateId.Should().Be("LevyDormancyTransitionComplete");
        sentCommands[0].RecipientsAddress.Should().Be("owner@test.com");
        sentCommands[0].Tokens["user_first_name"].Should().Be("Alex");
        sentCommands[0].Tokens["employer_name"].Should().Be("Test Employer");
        sentCommands[0].Tokens["switch_date"].Should().Be(now.ToString("dd MMMM yyyy"));

        var request = await dbContext.LevyDormancyRequests.SingleAsync();
        request.Status.Should().Be(LevyDormancyRequestStatus.Completed);
        request.ActionEmailSentAt.Should().Be(now);
    }

    [Test]
    public async Task Confirmation_email_uses_switched_account_name_not_another_account()
    {
        // Arrange
        var now = new DateTime(2026, 9, 1);
        var dbContext = CreateDbContext();
        await SeedAccount(dbContext, now, ApprenticeshipEmployerType.Levy, accountId: 1, name: "Switched Employer");
        await SeedAccount(dbContext, now, ApprenticeshipEmployerType.Levy, accountId: 2, name: "Other Employer");
        await SeedInProgressRequest(dbContext, now, warningSentAt: now.AddMonths(-1), accountId: 1);

        var accountRepository = new Mock<IEmployerAccountRepository>();
        var eventPublisher = new Mock<IEventPublisher>();
        eventPublisher
            .Setup(p => p.Publish(It.IsAny<ApprenticeshipEmployerTypeChangeEvent>()))
            .Returns(Task.CompletedTask);

        var sentCommands = new List<SendNotificationCommand>();
        var handler = CreateHandler(
            dbContext,
            new LevyDormancyConfiguration
            {
                OrchestrationEnabled = true,
                MonthsBetweenInitialWarningAndSwitch = 1
            },
            now,
            accountRepository.Object,
            eventPublisher.Object,
            sentCommands);

        // Act
        var result = await handler.Handle(new ProcessLevyDormancySwitchesCommand(), CancellationToken.None);

        // Assert
        result.AccountsSwitched.Should().Be(1);
        result.EmailsSent.Should().Be(1);

        sentCommands.Should().HaveCount(1);
        sentCommands[0].Tokens["employer_name"].Should().Be("Switched Employer");
        sentCommands[0].Tokens["employer_name"].Should().NotBe("Other Employer");

        accountRepository.Verify(
            r => r.SetAccountLevyStatus(1, ApprenticeshipEmployerType.NonLevy),
            Times.Once);
        accountRepository.Verify(
            r => r.SetAccountLevyStatus(2, It.IsAny<ApprenticeshipEmployerType>()),
            Times.Never);
    }

    [Test]
    public async Task Does_not_reswitch_when_action_email_already_sent()
    {
        // Arrange
        var now = new DateTime(2026, 9, 1);
        var dbContext = CreateDbContext();
        await SeedLevyAccount(dbContext, now);
        dbContext.LevyDormancyRequests.Add(new LevyDormancyRequest
        {
            AccountId = 1,
            NoLevyDeclaredMonths = 20,
            LastLevyDeclarationDate = now.AddMonths(-24),
            Status = LevyDormancyRequestStatus.Completed,
            CreatedOn = now.AddMonths(-4),
            UpdatedOn = now.AddMonths(-3),
            WarningEmailSentAt = now.AddMonths(-3),
            ActionEmailSentAt = now.AddMonths(-3)
        });
        await dbContext.SaveChangesAsync();
        var accountRepository = new Mock<IEmployerAccountRepository>();
        var eventPublisher = new Mock<IEventPublisher>();
        var sentCommands = new List<SendNotificationCommand>();
        var handler = CreateHandler(
            dbContext,
            new LevyDormancyConfiguration { OrchestrationEnabled = true },
            now,
            accountRepository.Object,
            eventPublisher.Object,
            sentCommands);

        // Act
        var result = await handler.Handle(new ProcessLevyDormancySwitchesCommand(), CancellationToken.None);

        // Assert
        result.RequestsProcessed.Should().Be(0);
        accountRepository.Verify(
            r => r.SetAccountLevyStatus(It.IsAny<long>(), It.IsAny<ApprenticeshipEmployerType>()),
            Times.Never);
        sentCommands.Should().BeEmpty();
    }

    [Test]
    public async Task Cancels_request_when_account_is_no_longer_levy()
    {
        // Arrange
        var now = new DateTime(2026, 9, 1);
        var dbContext = CreateDbContext();
        await SeedAccount(dbContext, now, ApprenticeshipEmployerType.NonLevy);
        await SeedInProgressRequest(dbContext, now, warningSentAt: now.AddMonths(-1));
        var accountRepository = new Mock<IEmployerAccountRepository>();
        var eventPublisher = new Mock<IEventPublisher>();
        var sentCommands = new List<SendNotificationCommand>();
        var handler = CreateHandler(
            dbContext,
            new LevyDormancyConfiguration
            {
                OrchestrationEnabled = true,
                MonthsBetweenInitialWarningAndSwitch = 1
            },
            now,
            accountRepository.Object,
            eventPublisher.Object,
            sentCommands);

        // Act
        var result = await handler.Handle(new ProcessLevyDormancySwitchesCommand(), CancellationToken.None);

        // Assert
        result.RequestsCancelled.Should().Be(1);
        accountRepository.Verify(
            r => r.SetAccountLevyStatus(It.IsAny<long>(), It.IsAny<ApprenticeshipEmployerType>()),
            Times.Never);
        eventPublisher.Verify(p => p.Publish(It.IsAny<ApprenticeshipEmployerTypeChangeEvent>()), Times.Never);
        sentCommands.Should().BeEmpty();
        (await dbContext.LevyDormancyRequests.SingleAsync()).Status.Should().Be(LevyDormancyRequestStatus.Cancelled);
    }

    [Test]
    public async Task Completes_switch_without_action_email_when_no_recipients()
    {
        // Arrange
        var now = new DateTime(2026, 9, 1);
        var dbContext = CreateDbContext();
        await SeedLevyAccount(dbContext, now);
        await SeedInProgressRequest(dbContext, now, warningSentAt: now.AddMonths(-1));
        var accountRepository = new Mock<IEmployerAccountRepository>();
        var eventPublisher = new Mock<IEventPublisher>();
        var sentCommands = new List<SendNotificationCommand>();
        var handler = CreateHandler(
            dbContext,
            new LevyDormancyConfiguration
            {
                OrchestrationEnabled = true,
                MonthsBetweenInitialWarningAndSwitch = 1
            },
            now,
            accountRepository.Object,
            eventPublisher.Object,
            sentCommands,
            teamMembers: []);

        // Act
        var result = await handler.Handle(new ProcessLevyDormancySwitchesCommand(), CancellationToken.None);

        // Assert
        result.AccountsSwitched.Should().Be(1);
        result.SkippedNoRecipients.Should().Be(1);
        result.EmailsSent.Should().Be(0);
        accountRepository.Verify(
            r => r.SetAccountLevyStatus(1, ApprenticeshipEmployerType.NonLevy),
            Times.Once);
        eventPublisher.Verify(p => p.Publish(It.IsAny<ApprenticeshipEmployerTypeChangeEvent>()), Times.Once);
        sentCommands.Should().BeEmpty();

        var request = await dbContext.LevyDormancyRequests.SingleAsync();
        request.Status.Should().Be(LevyDormancyRequestStatus.Completed);
        request.ActionEmailSentAt.Should().BeNull();
    }

    [Test]
    public async Task Does_not_retry_confirmation_email_when_switch_already_completed_without_email()
    {
        var now = new DateTime(2026, 9, 1);
        var dbContext = CreateDbContext();
        await SeedAccount(dbContext, now, ApprenticeshipEmployerType.NonLevy);
        dbContext.LevyDormancyRequests.Add(new LevyDormancyRequest
        {
            AccountId = 1,
            NoLevyDeclaredMonths = 20,
            LastLevyDeclarationDate = now.AddMonths(-24),
            Status = LevyDormancyRequestStatus.Completed,
            CreatedOn = now.AddMonths(-4),
            UpdatedOn = now.AddMonths(-3),
            WarningEmailSentAt = now.AddMonths(-3),
            ActionEmailSentAt = null
        });
        await dbContext.SaveChangesAsync();
        var accountRepository = new Mock<IEmployerAccountRepository>();
        var eventPublisher = new Mock<IEventPublisher>();
        var sentCommands = new List<SendNotificationCommand>();
        var handler = CreateHandler(
            dbContext,
            new LevyDormancyConfiguration { OrchestrationEnabled = true },
            now,
            accountRepository.Object,
            eventPublisher.Object,
            sentCommands);

        var result = await handler.Handle(new ProcessLevyDormancySwitchesCommand(), CancellationToken.None);

        result.RequestsProcessed.Should().Be(0);
        result.AccountsSwitched.Should().Be(0);
        result.EmailsSent.Should().Be(0);
        accountRepository.Verify(
            r => r.SetAccountLevyStatus(It.IsAny<long>(), It.IsAny<ApprenticeshipEmployerType>()),
            Times.Never);
        eventPublisher.Verify(p => p.Publish(It.IsAny<ApprenticeshipEmployerTypeChangeEvent>()), Times.Never);
        sentCommands.Should().BeEmpty();
        (await dbContext.LevyDormancyRequests.SingleAsync()).ActionEmailSentAt.Should().BeNull();
    }

    [Test]
    public async Task Switches_pending_request_immediately_when_skip_flag_is_on_and_switch_months_reached()
    {
        var now = new DateTime(2026, 9, 1);
        var dbContext = CreateDbContext();
        await SeedLevyAccount(dbContext, now);
        await SeedPendingRequest(dbContext, now, lastDeclaration: now.AddMonths(-24));
        var accountRepository = new Mock<IEmployerAccountRepository>();
        var eventPublisher = new Mock<IEventPublisher>();
        var sentCommands = new List<SendNotificationCommand>();
        var handler = CreateHandler(
            dbContext,
            CreateSkipInitialWarningConfiguration(),
            now,
            accountRepository.Object,
            eventPublisher.Object,
            sentCommands);

        var result = await handler.Handle(new ProcessLevyDormancySwitchesCommand(), CancellationToken.None);

        result.AccountsSwitched.Should().Be(1);
        result.EmailsSent.Should().Be(1);
        accountRepository.Verify(
            r => r.SetAccountLevyStatus(1, ApprenticeshipEmployerType.NonLevy),
            Times.Once);
        eventPublisher.Verify(p => p.Publish(It.IsAny<ApprenticeshipEmployerTypeChangeEvent>()), Times.Once);
        sentCommands.Should().HaveCount(1);
        sentCommands[0].TemplateId.Should().Be("LevyDormancyTransitionComplete");

        var request = await dbContext.LevyDormancyRequests.SingleAsync();
        request.Status.Should().Be(LevyDormancyRequestStatus.Completed);
        request.WarningEmailSentAt.Should().BeNull();
        request.ActionEmailSentAt.Should().Be(now);
    }

    [Test]
    public async Task Does_not_switch_pending_request_when_skip_flag_is_on_and_switch_months_not_reached()
    {
        var now = new DateTime(2026, 9, 1);
        var dbContext = CreateDbContext();
        await SeedLevyAccount(dbContext, now);
        await SeedPendingRequest(dbContext, now, lastDeclaration: now.AddMonths(-23));
        var accountRepository = new Mock<IEmployerAccountRepository>();
        var eventPublisher = new Mock<IEventPublisher>();
        var sentCommands = new List<SendNotificationCommand>();
        var handler = CreateHandler(
            dbContext,
            CreateSkipInitialWarningConfiguration(),
            now,
            accountRepository.Object,
            eventPublisher.Object,
            sentCommands);

        var result = await handler.Handle(new ProcessLevyDormancySwitchesCommand(), CancellationToken.None);

        result.SkippedNotYetEligible.Should().Be(1);
        result.AccountsSwitched.Should().Be(0);
        accountRepository.Verify(
            r => r.SetAccountLevyStatus(It.IsAny<long>(), It.IsAny<ApprenticeshipEmployerType>()),
            Times.Never);
        sentCommands.Should().BeEmpty();
        (await dbContext.LevyDormancyRequests.SingleAsync()).Status.Should().Be(LevyDormancyRequestStatus.Pending);
    }

    [Test]
    public async Task Does_not_reprocess_completed_skip_path_request_when_action_email_already_sent()
    {
        var now = new DateTime(2026, 9, 1);
        var dbContext = CreateDbContext();
        await SeedLevyAccount(dbContext, now);
        dbContext.LevyDormancyRequests.Add(new LevyDormancyRequest
        {
            AccountId = 1,
            NoLevyDeclaredMonths = 20,
            LastLevyDeclarationDate = now.AddMonths(-24),
            Status = LevyDormancyRequestStatus.Completed,
            CreatedOn = now.AddMonths(-4),
            UpdatedOn = now,
            WarningEmailSentAt = null,
            ActionEmailSentAt = now
        });
        await dbContext.SaveChangesAsync();
        var accountRepository = new Mock<IEmployerAccountRepository>();
        var eventPublisher = new Mock<IEventPublisher>();
        var sentCommands = new List<SendNotificationCommand>();
        var handler = CreateHandler(
            dbContext,
            CreateSkipInitialWarningConfiguration(),
            now,
            accountRepository.Object,
            eventPublisher.Object,
            sentCommands);

        var result = await handler.Handle(new ProcessLevyDormancySwitchesCommand(), CancellationToken.None);

        result.RequestsProcessed.Should().Be(0);
        accountRepository.Verify(
            r => r.SetAccountLevyStatus(It.IsAny<long>(), It.IsAny<ApprenticeshipEmployerType>()),
            Times.Never);
        sentCommands.Should().BeEmpty();
    }

    [Test]
    public async Task Does_not_reprocess_completed_skip_path_request_when_flag_is_later_turned_off()
    {
        var now = new DateTime(2026, 10, 1);
        var dbContext = CreateDbContext();
        await SeedLevyAccount(dbContext, now);
        dbContext.LevyDormancyRequests.Add(new LevyDormancyRequest
        {
            AccountId = 1,
            NoLevyDeclaredMonths = 20,
            LastLevyDeclarationDate = now.AddMonths(-25),
            Status = LevyDormancyRequestStatus.Completed,
            CreatedOn = now.AddMonths(-1),
            UpdatedOn = now.AddMonths(-1),
            WarningEmailSentAt = null,
            ActionEmailSentAt = now.AddMonths(-1)
        });
        await dbContext.SaveChangesAsync();
        var accountRepository = new Mock<IEmployerAccountRepository>();
        var eventPublisher = new Mock<IEventPublisher>();
        var sentCommands = new List<SendNotificationCommand>();
        var handler = CreateHandler(
            dbContext,
            new LevyDormancyConfiguration
            {
                OrchestrationEnabled = true,
                SkipInitialWarning = false,
                SwitchMonths = 24,
                MonthsBetweenInitialWarningAndSwitch = 1
            },
            now,
            accountRepository.Object,
            eventPublisher.Object,
            sentCommands);

        var result = await handler.Handle(new ProcessLevyDormancySwitchesCommand(), CancellationToken.None);

        result.RequestsProcessed.Should().Be(0);
        result.AccountsSwitched.Should().Be(0);
        accountRepository.Verify(
            r => r.SetAccountLevyStatus(It.IsAny<long>(), It.IsAny<ApprenticeshipEmployerType>()),
            Times.Never);
        eventPublisher.Verify(p => p.Publish(It.IsAny<ApprenticeshipEmployerTypeChangeEvent>()), Times.Never);
        sentCommands.Should().BeEmpty();
    }

    [Test]
    public async Task Does_not_switch_pending_request_when_skip_flag_is_off()
    {
        var now = new DateTime(2026, 9, 1);
        var dbContext = CreateDbContext();
        await SeedLevyAccount(dbContext, now);
        await SeedPendingRequest(dbContext, now, lastDeclaration: now.AddMonths(-24));
        var accountRepository = new Mock<IEmployerAccountRepository>();
        var eventPublisher = new Mock<IEventPublisher>();
        var sentCommands = new List<SendNotificationCommand>();
        var handler = CreateHandler(
            dbContext,
            new LevyDormancyConfiguration
            {
                OrchestrationEnabled = true,
                SkipInitialWarning = false,
                SwitchMonths = 24
            },
            now,
            accountRepository.Object,
            eventPublisher.Object,
            sentCommands);

        var result = await handler.Handle(new ProcessLevyDormancySwitchesCommand(), CancellationToken.None);

        result.RequestsProcessed.Should().Be(0);
        result.AccountsSwitched.Should().Be(0);
        accountRepository.Verify(
            r => r.SetAccountLevyStatus(It.IsAny<long>(), It.IsAny<ApprenticeshipEmployerType>()),
            Times.Never);
        sentCommands.Should().BeEmpty();
        (await dbContext.LevyDormancyRequests.SingleAsync()).Status.Should().Be(LevyDormancyRequestStatus.Pending);
    }

    [Test]
    public async Task Still_waits_after_warning_when_skip_flag_is_on()
    {
        var now = new DateTime(2026, 9, 1);
        var dbContext = CreateDbContext();
        await SeedLevyAccount(dbContext, now);
        await SeedInProgressRequest(dbContext, now, warningSentAt: now);
        var accountRepository = new Mock<IEmployerAccountRepository>();
        var eventPublisher = new Mock<IEventPublisher>();
        var sentCommands = new List<SendNotificationCommand>();
        var handler = CreateHandler(
            dbContext,
            CreateSkipInitialWarningConfiguration(),
            now,
            accountRepository.Object,
            eventPublisher.Object,
            sentCommands);

        var result = await handler.Handle(new ProcessLevyDormancySwitchesCommand(), CancellationToken.None);

        result.SkippedNotYetEligible.Should().Be(1);
        result.AccountsSwitched.Should().Be(0);
        accountRepository.Verify(
            r => r.SetAccountLevyStatus(It.IsAny<long>(), It.IsAny<ApprenticeshipEmployerType>()),
            Times.Never);
        sentCommands.Should().BeEmpty();
        (await dbContext.LevyDormancyRequests.SingleAsync()).Status.Should().Be(LevyDormancyRequestStatus.InProgress);
    }

    [Test]
    public async Task Switches_multiple_eligible_requests()
    {
        var now = new DateTime(2026, 9, 1);
        var dbContext = CreateDbContext();
        await SeedAccount(dbContext, now, ApprenticeshipEmployerType.Levy, accountId: 1);
        await SeedAccount(dbContext, now, ApprenticeshipEmployerType.Levy, accountId: 2);
        await SeedInProgressRequest(dbContext, now, warningSentAt: now.AddMonths(-1), accountId: 1);
        await SeedInProgressRequest(dbContext, now, warningSentAt: now.AddMonths(-1), accountId: 2);
        var accountRepository = new Mock<IEmployerAccountRepository>();
        var eventPublisher = new Mock<IEventPublisher>();
        var sentCommands = new List<SendNotificationCommand>();
        var handler = CreateHandler(
            dbContext,
            new LevyDormancyConfiguration
            {
                OrchestrationEnabled = true,
                MonthsBetweenInitialWarningAndSwitch = 1
            },
            now,
            accountRepository.Object,
            eventPublisher.Object,
            sentCommands);

        var result = await handler.Handle(new ProcessLevyDormancySwitchesCommand(), CancellationToken.None);

        result.RequestsProcessed.Should().Be(2);
        result.AccountsSwitched.Should().Be(2);
        result.EmailsSent.Should().Be(2);
        accountRepository.Verify(
            r => r.SetAccountLevyStatus(It.IsAny<long>(), ApprenticeshipEmployerType.NonLevy),
            Times.Exactly(2));
        eventPublisher.Verify(p => p.Publish(It.IsAny<ApprenticeshipEmployerTypeChangeEvent>()), Times.Exactly(2));
        dbContext.LevyDormancyRequests.Should().OnlyContain(r => r.Status == LevyDormancyRequestStatus.Completed);
    }

    private static EmployerAccountsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<EmployerAccountsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new EmployerAccountsDbContext(options);
    }

    private static LevyDormancyConfiguration CreateSkipInitialWarningConfiguration()
    {
        return new LevyDormancyConfiguration
        {
            OrchestrationEnabled = true,
            SkipInitialWarning = true,
            DormancyDetectionMonths = 20,
            InitialWarningMonths = 23,
            SwitchMonths = 24,
            MonthsBetweenInitialWarningAndSwitch = 1
        };
    }

    private static ProcessLevyDormancySwitchesCommandHandler CreateHandler(
        EmployerAccountsDbContext dbContext,
        LevyDormancyConfiguration levyDormancyConfiguration,
        DateTime now,
        IEmployerAccountRepository accountRepository,
        IEventPublisher eventPublisher,
        List<SendNotificationCommand> sentCommands,
        List<TeamMember> teamMembers = null)
    {
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

        return new ProcessLevyDormancySwitchesCommandHandler(
            new Lazy<EmployerAccountsDbContext>(() => dbContext),
            mediator.Object,
            accountRepository,
            accountTeamRepository.Object,
            eventPublisher,
            Options.Create(levyDormancyConfiguration),
            new CurrentDateTime(now),
            Mock.Of<ILogger<ProcessLevyDormancySwitchesCommandHandler>>());
    }

    private static async Task SeedLevyAccount(EmployerAccountsDbContext dbContext, DateTime assessedOn)
    {
        await SeedAccount(dbContext, assessedOn, ApprenticeshipEmployerType.Levy);
    }

    private static async Task SeedAccount(
        EmployerAccountsDbContext dbContext,
        DateTime assessedOn,
        ApprenticeshipEmployerType employerType,
        long accountId = 1,
        string name = "Test Employer")
    {
        dbContext.Accounts.Add(new Account
        {
            Id = accountId,
            Name = name,
            CreatedDate = assessedOn,
            ApprenticeshipEmployerType = (byte)employerType
        });

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedInProgressRequest(
        EmployerAccountsDbContext dbContext,
        DateTime now,
        DateTime warningSentAt,
        long accountId = 1)
    {
        dbContext.LevyDormancyRequests.Add(new LevyDormancyRequest
        {
            AccountId = accountId,
            NoLevyDeclaredMonths = 20,
            LastLevyDeclarationDate = now.AddMonths(-24),
            Status = LevyDormancyRequestStatus.InProgress,
            CreatedOn = warningSentAt.AddDays(-7),
            UpdatedOn = warningSentAt,
            WarningEmailSentAt = warningSentAt
        });

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedPendingRequest(
        EmployerAccountsDbContext dbContext,
        DateTime now,
        DateTime lastDeclaration)
    {
        dbContext.LevyDormancyRequests.Add(new LevyDormancyRequest
        {
            AccountId = 1,
            NoLevyDeclaredMonths = 20,
            LastLevyDeclarationDate = lastDeclaration,
            Status = LevyDormancyRequestStatus.Pending,
            CreatedOn = now.AddDays(-7),
            UpdatedOn = now.AddDays(-7)
        });

        await dbContext.SaveChangesAsync();
    }
}
