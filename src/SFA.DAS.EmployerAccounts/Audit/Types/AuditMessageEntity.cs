namespace SFA.DAS.EmployerAccounts.Audit.Types;

public class AuditMessageEntity
{
    public virtual Guid Id { get; set; } = Guid.NewGuid();
    public virtual string AffectedEntityType { get; set; }
    public virtual string AffectedEntityId { get; set; }
    public virtual string Category { get; set; }
    public virtual string Description { get; set; }
    public virtual string SourceSystem { get; set; }
    public virtual string SourceComponent { get; set; }
    public virtual string SourceVersion { get; set; }
    public virtual DateTime ChangedAt { get; set; }
    public virtual string ChangedById { get; set; }
    public virtual string ChangedByEmail { get; set; }
    public virtual string ChangedByOriginIp { get; set; }
}