﻿using System;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Models.UserProfile;
using SFA.DAS.EmployerAccounts.Services;

namespace SFA.DAS.EmployerAccounts.UnitTests.Services;

public class TestData
{
    public DateTime[] Attempts { get; set; }
    public int NumberOfPermittedAttempts { get; set; }
    public int PermittedAttemptsTimeSpanMinutes { get; set; }
    public int LockoutTimeSpanMinutes { get; set; }
    public UserAornPayeStatus Expected { get; set; }
}

[TestFixture]
public class UserAornPayeLockServiceTests
{
    private static readonly DateTime SeedDateTime = DateTime.UtcNow;

    private static readonly TestData[] TestData =
    [
        new()
        {
            NumberOfPermittedAttempts = 3,
            PermittedAttemptsTimeSpanMinutes = 10,
            LockoutTimeSpanMinutes = 30,
            Attempts = [SeedDateTime],
            Expected = new UserAornPayeStatus { IsLocked = false, RemainingAttempts = 2, AllowedAttempts = 3}
        },
        new()
        {
            NumberOfPermittedAttempts = 3,
            PermittedAttemptsTimeSpanMinutes = 10,
            LockoutTimeSpanMinutes = 30,
            Attempts = [SeedDateTime, SeedDateTime.AddMinutes(-1)],
            Expected = new UserAornPayeStatus { IsLocked = false, RemainingAttempts = 1, AllowedAttempts = 3 }
        },
        new()
        {
            NumberOfPermittedAttempts = 3,
            PermittedAttemptsTimeSpanMinutes = 10,
            LockoutTimeSpanMinutes = 30,
            Attempts = [],
            Expected = new UserAornPayeStatus { IsLocked = false, RemainingAttempts = 3, AllowedAttempts = 3 }
        },
        new()
        {
            NumberOfPermittedAttempts = 3,
            PermittedAttemptsTimeSpanMinutes = 10,
            LockoutTimeSpanMinutes = 30,
            Attempts = [SeedDateTime.AddMinutes(-5), SeedDateTime.AddMinutes(-6), SeedDateTime.AddMinutes(-7)],
            Expected = new UserAornPayeStatus { RemainingLock = 25, IsLocked = true, RemainingAttempts = 0, AllowedAttempts = 3 }
        },
        new()
        {
            NumberOfPermittedAttempts = 3,
            PermittedAttemptsTimeSpanMinutes = 10,
            LockoutTimeSpanMinutes = 30,
            Attempts = [SeedDateTime, SeedDateTime.AddMinutes(-6), SeedDateTime.AddMinutes(-10)],
            Expected = new UserAornPayeStatus { RemainingLock = 30, IsLocked = true, RemainingAttempts = 0, AllowedAttempts = 3 }
        },
        new()
        {
            NumberOfPermittedAttempts = 3,
            PermittedAttemptsTimeSpanMinutes = 10,
            LockoutTimeSpanMinutes = 30,
            Attempts = [SeedDateTime, SeedDateTime.AddMinutes(-5), SeedDateTime.AddMinutes(-20)],
            Expected = new UserAornPayeStatus { IsLocked = false, RemainingAttempts = 1, AllowedAttempts = 3 }
        },
        new()
        {
            NumberOfPermittedAttempts = 3,
            PermittedAttemptsTimeSpanMinutes = 10,
            LockoutTimeSpanMinutes = 30,
            Attempts = [SeedDateTime.AddMinutes(-9), SeedDateTime.AddMinutes(-10), SeedDateTime.AddMinutes(-11)],
            Expected = new UserAornPayeStatus { RemainingLock = 21, IsLocked = true, RemainingAttempts = 0, AllowedAttempts = 3 }
        },
        new()
        {
            NumberOfPermittedAttempts = 3,
            PermittedAttemptsTimeSpanMinutes = 10,
            LockoutTimeSpanMinutes = 30,
            Attempts = [SeedDateTime.AddMinutes(-11), SeedDateTime.AddMinutes(-12), SeedDateTime.AddMinutes(-13)],
            Expected = new UserAornPayeStatus { RemainingLock = 19, IsLocked = true, RemainingAttempts = 0, AllowedAttempts = 3 }
        },
        new()
        {
            NumberOfPermittedAttempts = 3,
            PermittedAttemptsTimeSpanMinutes = 10,
            LockoutTimeSpanMinutes = 30,
            Attempts = [SeedDateTime.AddMinutes(-4), SeedDateTime.AddMinutes(-11), SeedDateTime.AddMinutes(-12), SeedDateTime.AddMinutes(-13)],
            Expected = new UserAornPayeStatus { RemainingLock = 26, IsLocked = true, RemainingAttempts = 0, AllowedAttempts = 3 }
        },
        new()
        {
            NumberOfPermittedAttempts = 3,
            PermittedAttemptsTimeSpanMinutes = 10,
            LockoutTimeSpanMinutes = 30,
            Attempts = [SeedDateTime, SeedDateTime.AddMinutes(-11), SeedDateTime.AddMinutes(-12), SeedDateTime.AddMinutes(-13)],
            Expected = new UserAornPayeStatus { RemainingLock = 30, IsLocked = true, RemainingAttempts = 0, AllowedAttempts = 3 }
        },
        new()
        {
            NumberOfPermittedAttempts = 3,
            PermittedAttemptsTimeSpanMinutes = 10,
            LockoutTimeSpanMinutes = 30,
            Attempts = [SeedDateTime.AddMinutes(-1), SeedDateTime.AddMinutes(-2), SeedDateTime.AddMinutes(-13), SeedDateTime.AddMinutes(-14), SeedDateTime.AddMinutes(-15)],
            Expected = new UserAornPayeStatus { RemainingLock = 29, IsLocked = true, RemainingAttempts = 0, AllowedAttempts = 3 }
        }
    ];
     
    [Test]
    public void ItShouldReturnTheCorrectAornLockStatus([ValueSource(nameof(TestData))] TestData testData)
    {
        var userRef = Guid.NewGuid();
        var logger = Mock.Of<ILogger<UserAornPayeLockService>>();
        var userRepo = new Mock<IUserRepository>();
        userRepo.Setup(x => x.GetAornPayeQueryAttempts(It.IsAny<string>())).ReturnsAsync(testData.Attempts.ToList());

        var config = new EmployerAccountsConfiguration
        {
            UserAornPayeLock = new UserAornPayeLockConfiguration
            {
                NumberOfPermittedAttempts = testData.NumberOfPermittedAttempts,
                PermittedAttemptsTimeSpanMinutes = testData.PermittedAttemptsTimeSpanMinutes,
                LockoutTimeSpanMinutes = testData.LockoutTimeSpanMinutes
            }
        };

        var service = new UserAornPayeLockService(userRepo.Object, config, logger);
        var result = service.UserAornPayeStatus(userRef.ToString()).Result;

        testData.Expected.RemainingAttempts.Should().Be(result.RemainingAttempts);
        testData.Expected.IsLocked.Should().Be(result.IsLocked);
        testData.Expected.RemainingLock.Should().Be(result.RemainingLock);
    }
}