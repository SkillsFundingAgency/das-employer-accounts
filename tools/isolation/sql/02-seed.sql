USE [EmployerAccounts];
GO

/* Seed user/account aligned with StubId / StubEmail / WireMock ISOACC */
SET IDENTITY_INSERT [employer_account].[User] ON;
IF NOT EXISTS (SELECT 1 FROM [employer_account].[User] WHERE Id = 1)
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
IF NOT EXISTS (SELECT 1 FROM [employer_account].[Account] WHERE Id = 1)
BEGIN
    INSERT INTO [employer_account].[Account] (Id, HashedId, Name, CreatedDate, ModifiedDate, PublicHashedId, ApprenticeshipEmployerType, NameConfirmed, AddTrainingProviderAcknowledged)
    VALUES (
        1,
        'ISOACC',
        'Isolation Demo Account',
        GETUTCDATE(),
        GETUTCDATE(),
        'PUBISO',
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
GO
