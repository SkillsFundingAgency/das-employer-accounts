using SFA.DAS.EmployerAccounts.Dtos;

namespace SFA.DAS.EmployerAccounts.Queries.GetAccountEmployerAgreements;

public class GetAccountEmployerAgreementsResponse
{
    public List<EmployerAgreementStatusDto> EmployerAgreements { get; init; } = [];

    public bool HasPendingAgreements => EmployerAgreements != null && EmployerAgreements.Any(ag => ag.HasPendingAgreement);
    
    public bool HasAcknowledgedAgreements => EmployerAgreements != null && EmployerAgreements.Any(ag => ag.Acknowledged);

    public int MinimumSignedAgreementVersion => EmployerAgreements.Count != 0
        ? EmployerAgreements.Min(ea => ea.Signed?.VersionNumber ?? 0)
        : 0;
}