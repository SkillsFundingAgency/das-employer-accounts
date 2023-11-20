CREATE PROCEDURE [employer_account].[CreateAuditMessage] @Id uniqueidentifier,
                                                         @AffectedEntityType varchar(255),
                                                         @AffectedEntityId varchar(255),
                                                         @Category varchar(50),
                                                         @Description varchar(512),
                                                         @ChangedAt datetime,
                                                         @ChangedById varchar(150),
                                                         @ChangedByEmail varchar(255),
                                                         @ChangedByOriginIp varchar(50)
AS
INSERT INTO [employer_account].[AuditMessage]
(Id, AffectedEntityType, AffectedEntityId, Category, Description, ChangedAt, ChangedById, ChangedByEmail, ChangedByOriginIp)
VALUES (@Id, @AffectedEntityType, @AffectedEntityId, @Category, @Description, @ChangedAt, @ChangedById, @ChangedByEmail, @ChangedByOriginIp)
GO