using SFA.DAS.EmployerAccounts.Audit.Types;

namespace SFA.DAS.EmployerAccounts.Audit.MessageBuilders;
public interface IAuditMessageBuilder
{
    Task Build(AuditMessage message);
}
