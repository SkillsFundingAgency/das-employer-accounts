namespace SFA.DAS.EmployerAccounts.Commands.SignEmployerAgreementWithOutAudit;

public record SignEmployerAgreementWithoutAuditCommand(long AgreementId, User User, string CorrelationId) : IRequest;
