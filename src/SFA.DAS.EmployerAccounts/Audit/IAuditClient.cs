using SFA.DAS.EmployerAccounts.Audit.Types;

namespace SFA.DAS.EmployerAccounts.Audit;
public interface IAuditClient
{
    Task Audit(AuditMessage message);
}
