namespace SFA.DAS.EmployerAccounts.Queries.GetLegalEntity;

public class GetLegalEntityQuery : IRequest<GetLegalEntityResponse>
{
    public GetLegalEntityQuery(long accountId, long legalEntityId)
    {
        AccountId = accountId;
        LegalEntityId = legalEntityId;
    }

    public long AccountId { get; }

    public long LegalEntityId { get; }
}