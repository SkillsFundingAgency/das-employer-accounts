using SFA.DAS.EmployerAccounts.Audit.Types;

namespace SFA.DAS.EmployerAccounts.Data.Contracts;

public interface IAuditRepository
{
    Task Store(AuditMessage message);
}