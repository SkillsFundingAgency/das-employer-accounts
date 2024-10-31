namespace SFA.DAS.EmployerAccounts.Commands.SignEmployerAgreementWithOutAudit;

public record SignEmployerAgreementWithoutAuditCommand(string HashedAgreementId, User User, string CorrelationId) : IRequest;
