using SFA.DAS.EmployerAccounts.Audit.Types;

namespace SFA.DAS.EmployerAccounts.Audit.MessageBuilders;

public class BaseAuditMessageBuilder : IAuditMessageBuilder
{
    public Task Build(AuditMessage message)
    {
        message.ChangeAt = DateTime.UtcNow;
        
        return Task.CompletedTask;
    }
}