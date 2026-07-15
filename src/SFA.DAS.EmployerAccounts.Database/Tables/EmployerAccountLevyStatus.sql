CREATE TABLE [employer_account].[EmployerAccountLevyStatus]
(
    [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY,
    [AccountId] BIGINT NOT NULL,
    [LastLevyDeclarationDate] DATETIME NULL,
    [LastRefreshedAt] DATETIME NOT NULL
)
GO

CREATE UNIQUE INDEX [IX_EmployerAccountLevyStatus_AccountId]
ON [employer_account].[EmployerAccountLevyStatus] ([AccountId])
GO
