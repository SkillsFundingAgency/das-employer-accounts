CREATE TABLE [employer_account].[ChangedProperties](
	[PropertyName] [varchar](50) NOT NULL,
	[NewValue] [varchar](max) NULL,
	[MessageId] [uniqueidentifier] NOT NULL,
	CONSTRAINT [FK_ChangedProperties_AuditMessage] FOREIGN KEY (MessageId) REFERENCES [employer_account].[AuditMessage](Id)
)