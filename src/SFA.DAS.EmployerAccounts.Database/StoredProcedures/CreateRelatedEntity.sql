CREATE PROCEDURE [dbo].[CreateRelatedEntity]
	@EntityType varchar(255),
	@EntityId varchar(255),
	@MessageId uniqueidentifier
AS
	INSERT INTO RelatedEntities
	(EntityType, EntityId, MessageId)
	VALUES
	(@EntityType, @EntityId, @MessageId)

GO