namespace SFA.DAS.EmployerAccounts.Audit.Types;

public sealed class AuditMessageEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string AffectedEntityType { get; set; }
    public string AffectedEntityId { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public DateTime ChangedAt { get; set; }
    public string ChangedById { get; set; }
    public string ChangedByEmail { get; set; }
    public string ChangedByOriginIp { get; set; }
}