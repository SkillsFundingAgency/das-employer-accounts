using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SFA.DAS.EmployerAccounts.Audit.Types;
using SFA.DAS.EmployerAccounts.Data.Contracts;

namespace SFA.DAS.EmployerAccounts.Data;

public class AuditRepository : IAuditRepository
{
    private readonly Lazy<EmployerAccountsDbContext> _db;

    public AuditRepository(Lazy<EmployerAccountsDbContext> db) => _db = db;

    public async Task Store(AuditMessage message)
    {
        var messageEntity = MapDomainToEntity(message);

        await _db.Value.Database.GetDbConnection().ExecuteAsync(
            sql: "[employer_account].[CreateAuditMessage]",
            param: messageEntity,
            transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
            commandType: CommandType.StoredProcedure);
        
        foreach (var entity in message.RelatedEntities)
        {
            await _db.Value.Database.GetDbConnection().ExecuteAsync(
                sql: "[employer_account].[CreateRelatedEntity]",
                param: new { EntityType = entity.Type, EntityId = entity.Id, MessageId = messageEntity.Id },
                transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
                commandType: CommandType.StoredProcedure);
        }
        
        foreach (var update in message.ChangedProperties)
        {
            await _db.Value.Database.GetDbConnection().ExecuteAsync(
                sql: "[employer_account].[CreateChangedProperty]",
                param:  new { update.PropertyName, update.NewValue, MessageId = messageEntity.Id },
                transaction: _db.Value.Database.CurrentTransaction?.GetDbTransaction(),
                commandType: CommandType.StoredProcedure);
        }
    }

    private static AuditMessageEntity MapDomainToEntity(AuditMessage message)
    {
        return new AuditMessageEntity
        {
            AffectedEntityId = message.AffectedEntity.Id,
            AffectedEntityType = message.AffectedEntity.Type,
            Category = message.Category,
            Description = message.Description,
            SourceSystem = message.Source.System,
            SourceComponent = message.Source.Component,
            SourceVersion = message.Source.Version,
            ChangedAt = message.ChangeAt,
            ChangedById = message.ChangedBy?.Id,
            ChangedByEmail = message.ChangedBy?.EmailAddress,
            ChangedByOriginIp = message.ChangedBy?.OriginIpAddress
        };
    }
}