CREATE TABLE [dbo].[RelatedEntities](
	[EntityType] [varchar](255) NOT NULL,
	[EntityId] [varchar](255) NOT NULL,
	[MessageId] [uniqueidentifier] NOT NULL,
	CONSTRAINT [FK_RelatedEntities_AuditMessage] FOREIGN KEY (MessageId) REFERENCES [dbo].[AuditMessage](Id)
)
GO

CREATE CLUSTERED INDEX [IX_RelatedEntities_Entity] ON [dbo].[RelatedEntities] (EntityType, EntityId)
GO