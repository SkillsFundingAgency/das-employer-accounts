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