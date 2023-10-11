CREATE PROCEDURE [dbo].[CreateChangedProperty]
	@PropertyName varchar(50),
	@NewValue varchar(max),
	@MessageId uniqueidentifier
AS
	INSERT INTO ChangedProperties
	(PropertyName, NewValue, MessageId)
	VALUES
	(@PropertyName, @NewValue, @MessageId)

GO