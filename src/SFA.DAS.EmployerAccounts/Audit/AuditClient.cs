using SFA.DAS.EmployerAccounts.Audit.Types;
using SFA.DAS.EmployerAccounts.Data.Contracts;

namespace SFA.DAS.EmployerAccounts.Audit;

public class AuditClient : IAuditClient
{
    private readonly IAuditRepository _auditRepository;

    public AuditClient(IAuditRepository auditRepository) => _auditRepository = auditRepository;


    public async Task Audit(AuditMessage message)
    {
        await _auditRepository.Store(message);
    }
}