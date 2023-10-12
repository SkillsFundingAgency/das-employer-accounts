CREATE TABLE [employer_account].[AuditMessage]
(
    [Id]                 [uniqueidentifier] NOT NULL,
    [AffectedEntityType] [varchar](255)     NOT NULL,
    [AffectedEntityId]   [varchar](255)     NOT NULL,
    [Category]           [varchar](50)      NOT NULL,
    [Description]        [varchar](512)     NOT NULL,
    [SourceSystem]       [varchar](50)      NOT NULL,
    [SourceComponent]    [varchar](50)      NOT NULL,
    [SourceVersion]      [varchar](25)      NOT NULL,
    [ChangedAt]          [datetime]         NOT NULL,
    [ChangedById]        [varchar](150)     NULL,
    [ChangedByEmail]     [varchar](255)     NULL,
    [ChangedByOriginIp]  [varchar](50)      NULL,
    CONSTRAINT [PK_AuditMessage] PRIMARY KEY NONCLUSTERED (Id)
)
GO

CREATE CLUSTERED INDEX [IX_AuditMessage_AffectedEntity] ON [employer_account].[AuditMessage] (AffectedEntityType, AffectedEntityId)
GO
