SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO
/* Isolation schema bootstrap - FAKE local data only. Prefer publishing the DACPAC when available. */
IF DB_ID(N'EmployerAccounts') IS NULL CREATE DATABASE [EmployerAccounts];
GO
USE [EmployerAccounts];
GO
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'employer_account') EXEC('CREATE SCHEMA [employer_account]');
GO
-- ===== Account.sql =====
CREATE TABLE [employer_account].[Account]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
	[HashedId] NVARCHAR(100) NULL,
    [Name] NVARCHAR(100) NOT NULL ,
	[CreatedDate] DATETIME NOT NULL,
	[ModifiedDate] DATETIME NULL, 
    [PublicHashedId] NVARCHAR(100) NULL, 
    [ApprenticeshipEmployerType] TINYINT NOT NULL, 
    [NameConfirmed] BIT NULL DEFAULT(0),
    [AddTrainingProviderAcknowledged] BIT NULL DEFAULT(0)
)
GO

CREATE INDEX [IX_Account_HashedAccountId] ON [employer_account].[Account] ([HashedId])
GO

CREATE UNIQUE INDEX [IX_Account_PublicHashedAccountId] ON [employer_account].[Account] ([PublicHashedId]) WHERE PublicHashedId IS NOT NULL
GO

CREATE INDEX [IX_Account_ApprenticeshipEmployerType] ON [employer_account].[Account] ([ApprenticeshipEmployerType])
GO
-- ===== User.sql =====
CREATE TABLE [employer_account].[User]
(
    [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [UserRef] UNIQUEIDENTIFIER NOT NULL, 
    [Email] NVARCHAR(255) NOT NULL UNIQUE, 
    [FirstName] NVARCHAR(MAX) NULL, 
    [LastName] NVARCHAR(MAX) NULL, 
    [CorrelationId] NVARCHAR(255) NULL, 
    [TermAndConditionsAcceptedOn] DATETIME NULL,
    [LastLogin] DATETIME2 NULL
)
GO

CREATE NONCLUSTERED INDEX [IX_User_UserRef] ON [employer_account].[User] ([UserRef])
GO

CREATE NONCLUSTERED INDEX [IX_User_Email] ON [employer_account].[User] ([Email]) INCLUDE ([UserRef])
GO

-- ===== Membership.sql =====
CREATE TABLE [employer_account].[Membership]
(
    [AccountId] BIGINT NOT NULL, 
    [UserId] BIGINT NOT NULL, 
    [Role] INT NOT NULL, 
	[CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [ShowWizard] BIT NOT NULL DEFAULT 1, 
    CONSTRAINT [FK_Membership_Account] FOREIGN KEY (AccountId) REFERENCES [employer_account].[Account]([Id]), 
    CONSTRAINT [FK_Membership_User] FOREIGN KEY (UserId) REFERENCES [employer_account].[User]([Id]), 
    CONSTRAINT [PK_Membership] PRIMARY KEY ([UserId], [AccountId])
)

GO

CREATE NONCLUSTERED INDEX [IX_Membership_AccountIdRoleId] ON [employer_account].[Membership] ([AccountId], [Role]) INCLUDE ([CreatedDate])

GO
CREATE NONCLUSTERED INDEX [IX_Membership_UserId] ON [employer_account].[Membership] ([UserId])

GO
-- ===== LegalEntity.sql =====
CREATE TABLE [employer_account].[LegalEntity]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [Code] NVARCHAR(50) NULL, 
    [DateOfIncorporation] DATETIME NULL,
	[Status] NVARCHAR(50) NULL,
	[Source] SMALLINT NOT NULL DEFAULT 1,
	[PublicSectorDataSource] TINYINT NULL,
	[Sector] NVARCHAR(100) NULL
)
GO
CREATE INDEX [IX_LegalEntity_Code_Source] ON [employer_account].[LegalEntity]([Code], [Source]) INCLUDE ([Id])
GO
-- ===== AccountLegalEntity.sql =====
CREATE TABLE [employer_account].[AccountLegalEntity]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [Name] NVARCHAR(100) NOT NULL, 
    [Address] NVARCHAR(256) NULL, 
    [AccountId] BIGINT NOT NULL, 
    [LegalEntityId] BIGINT NOT NULL, 
    [Created] DATETIME NOT NULL, 
    [Modified] DATETIME NULL, 
    [SignedAgreementVersion] INT NULL, 
    [SignedAgreementId] BIGINT NULL, 
    [PendingAgreementVersion] INT NULL, 
    [PendingAgreementId] BIGINT NULL,
    [PublicHashedId] NVARCHAR(6) NULL, 
    [Deleted] DATETIME NULL, 
    CONSTRAINT [FK_AccountLegalEntity_Account] FOREIGN KEY ([AccountId]) REFERENCES [Employer_Account].[Account]([Id]),
    CONSTRAINT [FK_AccountLegalEntity_LegalEntity] FOREIGN KEY ([LegalEntityId]) REFERENCES [Employer_Account].[LegalEntity]([Id])
)

GO

CREATE INDEX [IX_AccountLegalEntity_AccountId] ON [employer_account].[AccountLegalEntity] ([AccountId]);
GO

CREATE INDEX [IX_AccountLegalEntity_LegalEntityId] ON [employer_account].[AccountLegalEntity] ([LegalEntityId]);
GO 

CREATE UNIQUE INDEX [IX_AccountLegalEntity_AccountIdLegalEntityId] ON [employer_account].[AccountLegalEntity] (AccountId, LegalEntityId) WHERE Deleted IS NULL;
GO

CREATE UNIQUE INDEX [IX_AccountLegalEntity_PublicHashedId] ON [employer_account].[AccountLegalEntity] ([PublicHashedId]) WHERE PublicHashedId IS NOT NULL AND Deleted IS NULL;
GO


-- ===== EmployerAgreementTemplate.sql =====
CREATE TABLE [employer_account].[EmployerAgreementTemplate]
(
    [Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [PartialViewName] NVARCHAR(50) NOT NULL,
    [CreatedDate] DATETIME NOT NULL, 
    [VersionNumber] INT NOT NULL,
	[AgreementType] TINYINT NOT NULL DEFAULT 0,
	[PublishedDate]  DATETIME NULL
)
GO

CREATE UNIQUE INDEX [IX_AgreementTypeVersionNumber]
ON [employer_account].[EmployerAgreementTemplate] (AgreementType, [VersionNumber] DESC)
GO

-- ===== EmployerAgreement.sql =====
CREATE TABLE [employer_account].[EmployerAgreement]
(
    [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [TemplateId] INT NOT NULL, 
    [StatusId] TINYINT NOT NULL DEFAULT 1, 
    [SignedByName] NVARCHAR(100) NULL, 
    [SignedDate] DATETIME NULL, 
	[AccountLegalEntityId] BIGINT NOT NULL,
    [ExpiredDate] DATETIME NULL, 
    [SignedById] BIGINT NULL,
    [Acknowledged] BIT NULL DEFAULT(0),
    CONSTRAINT [FK_EmployerAgreement_AccountLegalEntity] FOREIGN KEY ([AccountLegalEntityId]) REFERENCES [employer_account].[AccountLegalEntity]([Id]), 
    CONSTRAINT [FK_EmployerAgreement_SignedBy] FOREIGN KEY ([SignedById]) REFERENCES [employer_account].[User]([Id])
)
GO

CREATE UNIQUE INDEX [IX_EmployerAgreement_LegalEntity]
ON [employer_account].[EmployerAgreement] (AccountLegalEntityId, TemplateId)
WHERE StatusId <> 5
GO
CREATE INDEX [IX_EmployerAgreement_LegalEntityStatus]
ON [employer_account].[EmployerAgreement] (AccountLegalEntityId, StatusId)
GO
-- ===== Paye.sql =====
CREATE TABLE [employer_account].[Paye]
(
	[Ref] NVARCHAR(16) NOT NULL PRIMARY KEY, 
	[AccessToken] VARCHAR(50) NULL,
	[RefreshToken] VARCHAR(50) NULL,
	[Name] VARCHAR(500) NULL, 
    [Aorn] VARCHAR(25) NULL 
)

GO
-- ===== AccountHistory.sql =====
CREATE TABLE [employer_account].[AccountHistory]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY,
	[AccountId] BIGINT NOT NULL,
	[PayeRef] VARCHAR(20) NOT NULL,
	[AddedDate] DATETIME NOT NULL,
	[RemovedDate] DATETIME NULL,
	CONSTRAINT [FK_AccountHistory_Account] FOREIGN KEY (AccountId) REFERENCES [employer_account].[Account]([Id])
)
GO

CREATE INDEX [IX_AccountId_PayeRef] ON [employer_account].[AccountHistory] (AccountId, PayeRef)
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_PayeRef_RemovedDate] ON [employer_account].[AccountHistory] (PayeRef) WHERE RemovedDate IS NULL
GO

CREATE NONCLUSTERED INDEX [IX_AccountHistory_RemovedDate] ON [employer_account].[AccountHistory] ([RemovedDate]) INCLUDE ([AccountId], [AddedDate], [PayeRef])
GO
-- ===== Invitation.sql =====
CREATE TABLE [employer_account].[Invitation](
	[Id] BIGINT IDENTITY(1,1) NOT NULL,
	[AccountId] BIGINT NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Email] [nvarchar](255) NOT NULL,
	[ExpiryDate] [datetime] NOT NULL,
	[Status] [tinyint] NOT NULL,
 [Role] TINYINT NOT NULL, 
    CONSTRAINT [PK_Invitation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY], 
    CONSTRAINT [FK_Invitation_Account] FOREIGN KEY ([AccountId]) REFERENCES [employer_account].[Account]([Id])
) ON [PRIMARY]

GO
CREATE INDEX [IX_Invitation] ON [employer_account].[Invitation] ([Email], [Status], [ExpiryDate]) INCLUDE ([AccountId], [Name], [Role])

GO
CREATE INDEX [IX_Invitation_AccountId_Status] ON [employer_account].[Invitation]([AccountId], [Status]) INCLUDE ([Email], [ExpiryDate],	[Name],	[Role])
GO
-- ===== UserAccountSettings.sql =====
CREATE TABLE [employer_account].[UserAccountSettings]
(
	[Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[UserId] BIGINT NOT NULL,
	[AccountId] BIGINT NOT NULL,
	[ReceiveNotifications] BIT NOT NULL DEFAULT(1),
	CONSTRAINT [FK_UserAccountSettings_UserId] FOREIGN KEY(UserId) REFERENCES [employer_account].[User] ([Id]),
	CONSTRAINT [FK_UserAccountSettings_AccountId] FOREIGN KEY(AccountId) REFERENCES [employer_account].[Account] ([Id])
)
GO

CREATE UNIQUE INDEX [IX_UserAccountSettings] ON [employer_account].[UserAccountSettings] ([UserId], [AccountId]) INCLUDE ([ReceiveNotifications])
GO
-- ===== UserAornFailedAttempts.sql =====
CREATE TABLE [employer_account].[UserAornFailedAttempts](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UserId] [bigint] NOT NULL,
	[AttemptTimeStamp] [datetime] NOT NULL,
 CONSTRAINT [PK_UserAornFailedAttempt] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [employer_account].[UserAornFailedAttempts]  WITH CHECK ADD  CONSTRAINT [FK_UserAornFailedAttempt_User] FOREIGN KEY([UserId])
REFERENCES [employer_account].[User] ([Id])
GO

ALTER TABLE [employer_account].[UserAornFailedAttempts] CHECK CONSTRAINT [FK_UserAornFailedAttempt_User]
GO
CREATE NONCLUSTERED INDEX [IX_UserAornFailedAttempts_UserId] ON [employer_account].[UserAornFailedAttempts] ([UserId])
GO
-- ===== EmployerAccountLevyStatus.sql =====
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

CREATE INDEX [IX_EmployerAccountLevyStatus_LastLevyDeclarationDate]
ON [employer_account].[EmployerAccountLevyStatus] ([LastLevyDeclarationDate])
INCLUDE ([AccountId])
GO

-- ===== LevyDormancyRequest.sql =====
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

-- ===== HealthChecks.sql =====
CREATE TABLE [dbo].[HealthChecks]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[UserRef] UNIQUEIDENTIFIER NOT NULL,
	[SentRequest] DATETIME NOT NULL,
	[ReceivedResponse] DATETIME NULL,
	[PublishedEvent] DATETIME NOT NULL,
	[ReceivedEvent] DATETIME NULL
)
GO
-- ===== AuditMessage.sql =====
CREATE TABLE [employer_account].[AuditMessage]
(
    [Id]                 [uniqueidentifier] NOT NULL,
    [AffectedEntityType] [varchar](255)     NOT NULL,
    [AffectedEntityId]   [varchar](255)     NOT NULL,
    [Category]           [varchar](50)      NOT NULL,
    [Description]        [varchar](512)     NOT NULL,
    [ChangedAt]          [datetime]         NOT NULL,
    [ChangedById]        [varchar](150)     NULL,
    [ChangedByEmail]     [varchar](255)     NULL,
    [ChangedByOriginIp]  [varchar](50)      NULL,
    CONSTRAINT [PK_AuditMessage] PRIMARY KEY NONCLUSTERED (Id)
)
GO

CREATE CLUSTERED INDEX [IX_AuditMessage_AffectedEntity] ON [employer_account].[AuditMessage] (AffectedEntityType, AffectedEntityId)
GO

-- ===== ChangedProperties.sql =====
CREATE TABLE [employer_account].[ChangedProperties](
	[PropertyName] [varchar](50) NOT NULL,
	[NewValue] [varchar](max) NULL,
	[MessageId] [uniqueidentifier] NOT NULL,
	CONSTRAINT [FK_ChangedProperties_AuditMessage] FOREIGN KEY (MessageId) REFERENCES [employer_account].[AuditMessage](Id)
)
GO
-- ===== RelatedEntities.sql =====
CREATE TABLE [employer_account].[RelatedEntities]
(
    [EntityType] [varchar](255)     NOT NULL,
    [EntityId]   [varchar](255)     NOT NULL,
    [MessageId]  [uniqueidentifier] NOT NULL,
    CONSTRAINT [FK_RelatedEntities_AuditMessage] FOREIGN KEY (MessageId) REFERENCES [employer_account].[AuditMessage] (Id)
)
GO

CREATE CLUSTERED INDEX [IX_RelatedEntities_Entity] ON [employer_account].[RelatedEntities] (EntityType, EntityId)
GO
-- ===== RunOnceJob.sql =====
CREATE TABLE [dbo].[RunOnceJob]
(
	[Name] NVARCHAR(50) NOT NULL PRIMARY KEY, 
	[Completed] DATETIME2 NOT NULL
)
GO
-- ===== OutboxData.sql =====
CREATE TABLE [dbo].[OutboxData]
(
	[MessageId] NVARCHAR(200) NOT NULL PRIMARY KEY NONCLUSTERED,
	[Dispatched] BIT NOT NULL DEFAULT(0),
	[DispatchedAt] DATETIME NULL,
	[PersistenceVersion] VARCHAR(23) NOT NULL,
	[Operations] NVARCHAR(MAX) NOT NULL,
)
GO

CREATE INDEX [IX_DispatchedAt] ON [dbo].[OutboxData] ([DispatchedAt] ASC) WHERE [Dispatched] = 1
GO
-- ===== ClientOutboxData.sql =====
CREATE TABLE [dbo].[ClientOutboxData]
(
	[MessageId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY NONCLUSTERED,
	[EndpointName] NVARCHAR(150) NOT NULL,
	[CreatedAt] DATETIME NOT NULL,
	[Dispatched] BIT NOT NULL DEFAULT(0),
	[DispatchedAt] DATETIME NULL,
	[Operations] NVARCHAR(MAX) NOT NULL, 
    [PersistenceVersion] VARCHAR(23) NOT NULL DEFAULT '1.0.0'
)
GO

CREATE INDEX [IX_CreatedAt] ON [dbo].[ClientOutboxData] ([CreatedAt] ASC) WHERE [Dispatched] = 0
GO

CREATE INDEX [IX_DispatchedAt] ON [dbo].[ClientOutboxData] ([DispatchedAt] ASC) WHERE [Dispatched] = 1
GO
