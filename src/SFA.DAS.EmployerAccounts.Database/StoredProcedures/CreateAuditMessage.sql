CREATE PROCEDURE [dbo].[CreateAuditMessage]
	@Id uniqueidentifier,
	@AffectedEntityType varchar(255),
	@AffectedEntityId varchar(255),
	@Category varchar(50),
	@Description varchar(512),
	@SourceSystem varchar(50),
	@SourceComponent varchar(50),
	@SourceVersion varchar(25),
	@ChangedAt datetime,
	@ChangedById varchar(150),
	@ChangedByEmail varchar(255),
	@ChangedByOriginIp varchar(50)
AS
	INSERT INTO [dbo].[AuditMessage]
	(Id,AffectedEntityType,AffectedEntityId,Category,Description,SourceSystem,SourceComponent,SourceVersion,ChangedAt,ChangedById,ChangedByEmail,ChangedByOriginIp)
	VALUES
	(@Id,@AffectedEntityType,@AffectedEntityId,@Category,@Description,@SourceSystem,@SourceComponent,@SourceVersion,@ChangedAt,@ChangedById,@ChangedByEmail,@ChangedByOriginIp)
GO