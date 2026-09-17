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
