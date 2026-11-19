CREATE TABLE [employer_account].[LevyDormancyRequest]
(
    [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY,
    [AccountId] BIGINT NOT NULL,
    [NoLevyDeclaredMonths] INT NOT NULL,
    [LastLevyDeclarationDate] DATETIME NULL,
    [Status] TINYINT NOT NULL,
    [CreatedOn] DATETIME NOT NULL,
    [UpdatedOn] DATETIME NOT NULL,
    [WarningEmailSentAt] DATETIME NULL,
    [FinalWarningEmailSentAt] DATETIME NULL,
    [ActionEmailSentAt] DATETIME NULL
)
GO

CREATE INDEX [IX_LevyDormancyRequest_AccountId]
ON [employer_account].[LevyDormancyRequest] ([AccountId])
GO

CREATE INDEX [IX_LevyDormancyRequest_AccountId_Status]
ON [employer_account].[LevyDormancyRequest] ([AccountId], [Status])
GO

CREATE INDEX [IX_LevyDormancyRequest_Status]
ON [employer_account].[LevyDormancyRequest] ([Status])
INCLUDE ([AccountId])
GO
