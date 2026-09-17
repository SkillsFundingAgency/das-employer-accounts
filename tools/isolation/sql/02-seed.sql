USE [EmployerAccounts];
GO

/* Seed user/account aligned with StubId / StubEmail / WireMock GP67XW.
   Overwrites Id=1 if DACPAC PostDeploy / SeedDevData left TEST hashes. */
SET IDENTITY_INSERT [employer_account].[User] ON;
IF EXISTS (SELECT 1 FROM [employer_account].[User] WHERE Id = 1)
BEGIN
    UPDATE [employer_account].[User]
    SET UserRef = '11111111-1111-1111-1111-111111111111',
        Email = 'isolation.employer@example.com',
        FirstName = 'Isolation',
        LastName = 'Employer',
        TermAndConditionsAcceptedOn = GETUTCDATE(),
        LastLogin = GETUTCDATE()
    WHERE Id = 1;
END
ELSE
BEGIN
    INSERT INTO [employer_account].[User] (Id, UserRef, Email, FirstName, LastName, TermAndConditionsAcceptedOn, LastLogin)
    VALUES (
        1,
        '11111111-1111-1111-1111-111111111111',
        'isolation.employer@example.com',
        'Isolation',
        'Employer',
        GETUTCDATE(),
        GETUTCDATE()
    );
END
SET IDENTITY_INSERT [employer_account].[User] OFF;
GO

SET IDENTITY_INSERT [employer_account].[Account] ON;
IF EXISTS (SELECT 1 FROM [employer_account].[Account] WHERE Id = 1)
BEGIN
    UPDATE [employer_account].[Account]
    SET HashedId = 'GP67XW',
        Name = 'Isolation Demo Account',
        ModifiedDate = GETUTCDATE(),
        PublicHashedId = 'JYB49M',
        ApprenticeshipEmployerType = 1,
        NameConfirmed = 1,
        AddTrainingProviderAcknowledged = 1
    WHERE Id = 1;
END
ELSE
BEGIN
    INSERT INTO [employer_account].[Account] (Id, HashedId, Name, CreatedDate, ModifiedDate, PublicHashedId, ApprenticeshipEmployerType, NameConfirmed, AddTrainingProviderAcknowledged)
    VALUES (
        1,
        'GP67XW',
        'Isolation Demo Account',
        GETUTCDATE(),
        GETUTCDATE(),
        'JYB49M',
        1, -- NonLevy
        1,
        1
    );
END
SET IDENTITY_INSERT [employer_account].[Account] OFF;
GO

IF NOT EXISTS (SELECT 1 FROM [employer_account].[Membership] WHERE AccountId = 1 AND UserId = 1)
BEGIN
    INSERT INTO [employer_account].[Membership] (AccountId, UserId, Role, CreatedDate, ShowWizard)
    VALUES (1, 1, 1 /* Owner */, GETUTCDATE(), 0);
END
ELSE
BEGIN
    UPDATE [employer_account].[Membership]
    SET Role = 1, ShowWizard = 0
    WHERE AccountId = 1 AND UserId = 1;
END
GO

-- ===== dashboard stubs (was 03-dashboard-stubs.sql) =====
-- Isolation dashboard stubs: views/procs required by EmployerTeamOrchestrator.GetAccount
-- Adapted from Database project for the isolation schema.

IF OBJECT_ID(N'[employer_account].[MembershipView]', N'V') IS NOT NULL DROP VIEW [employer_account].[MembershipView];
GO
CREATE VIEW [employer_account].[MembershipView]
AS
SELECT
    m.AccountId,
    m.UserId,
    m.Role,
    m.CreatedDate,
    m.ShowWizard,
    u.UserRef,
    u.Email,
    u.FirstName,
    u.LastName,
    u.CorrelationId,
    a.Name AS AccountName,
    a.HashedId AS HashedAccountId
FROM [employer_account].[Membership] AS m
INNER JOIN [employer_account].[User] AS u ON u.Id = m.UserId
INNER JOIN [employer_account].[Account] AS a ON a.Id = m.AccountId;
GO

IF OBJECT_ID(N'[employer_account].[GetTeamMembers]', N'V') IS NOT NULL DROP VIEW [employer_account].[GetTeamMembers];
GO
CREATE VIEW [employer_account].[GetTeamMembers]
AS
SELECT
    1 AS IsUser,
    a.Id AS AccountId,
    a.HashedId,
    u.FirstName,
    u.LastName,
    u.Id,
    CONCAT(u.FirstName, N' ', u.LastName) AS Name,
    u.Email,
    u.UserRef,
    m.Role AS Role,
    2 AS Status,
    CAST(NULL AS DATETIME) AS ExpiryDate
FROM [employer_account].[User] u
LEFT JOIN [employer_account].[Membership] m ON m.UserId = u.Id
LEFT JOIN [employer_account].[Account] a ON a.Id = m.AccountId
UNION ALL
SELECT
    0,
    i.AccountId,
    a.HashedId,
    N'' AS FirstName,
    N'' AS LastName,
    i.Id,
    i.Name,
    i.Email,
    CAST(NULL AS UNIQUEIDENTIFIER),
    i.Role,
    CASE WHEN i.Status = 1 AND i.ExpiryDate < GETDATE() THEN 3 ELSE i.Status END AS Status,
    i.ExpiryDate
FROM [employer_account].[Invitation] i
JOIN [employer_account].[Account] a ON a.Id = i.AccountId
WHERE i.Status NOT IN (2, 4);
GO

IF OBJECT_ID(N'[employer_account].[GetInvitations]', N'V') IS NOT NULL DROP VIEW [employer_account].[GetInvitations];
GO
CREATE VIEW [employer_account].[GetInvitations]
AS
SELECT
    i.Id,
    i.AccountId,
    a.Name AS AccountName,
    i.Name,
    i.Email,
    i.ExpiryDate,
    CASE WHEN i.Status = 1 AND i.ExpiryDate < GETDATE() THEN 3 ELSE i.Status END AS Status,
    i.Role,
    u.Id AS InternalUserId,
    u.UserRef AS ExternalUserId
FROM [employer_account].[Invitation] i
JOIN [employer_account].[Account] a ON a.Id = i.AccountId
LEFT OUTER JOIN [employer_account].[User] u ON u.Email = i.Email;
GO

CREATE OR ALTER PROCEDURE [employer_account].[GetAccountStats]
    @accountId BIGINT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        (SELECT @accountId) AS AccountId,
        (SELECT COUNT(1) FROM [employer_account].[AccountHistory] WHERE AccountId = @accountId AND RemovedDate IS NULL) AS PayeSchemeCount,
        (SELECT COUNT(1) FROM [employer_account].[AccountLegalEntity] WHERE AccountId = @accountId AND Deleted IS NULL) AS OrganisationCount,
        (SELECT COUNT(1) FROM [employer_account].[Membership] WHERE AccountId = @accountId) AS TeamMemberCount,
        (SELECT COUNT(1) FROM [employer_account].[Invitation] WHERE AccountId = @accountId AND Status = 1) AS TeamMembersInvited;
END
GO

CREATE OR ALTER PROCEDURE [employer_account].[GetTeamMember]
    @hashedAccountId NVARCHAR(50),
    @externalUserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT *
    FROM [employer_account].[MembershipView] m
    WHERE m.HashedAccountId = @hashedAccountId AND m.UserRef = @externalUserId;
END
GO

CREATE OR ALTER PROCEDURE [employer_account].[GetEmployerAccountMembers]
    @accountId BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        tm.*,
        s.ReceiveNotifications AS CanReceiveNotifications
    FROM [employer_account].[GetTeamMembers] tm
    LEFT JOIN [employer_account].[UserAccountSettings] s
        ON tm.Id = s.UserId AND tm.AccountId = s.AccountId
    WHERE tm.AccountId = @accountId;
END
GO

-- Seed settings row for isolation user if missing
IF NOT EXISTS (
    SELECT 1 FROM [employer_account].[UserAccountSettings]
    WHERE UserId = 1 AND AccountId = 1
)
BEGIN
    INSERT INTO [employer_account].[UserAccountSettings] (UserId, AccountId, ReceiveNotifications)
    VALUES (1, 1, 1);
END
GO
