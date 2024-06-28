namespace SFA.DAS.EmployerAccounts.Queries.GetAccountLegalEntitiesByHashedAccountId;

public class GetAccountLegalEntitiesByHashedAccountIdRequest : IRequest<GetAccountLegalEntitiesByHashedAccountIdResponse>
{
    public long AccountId { get; set; }
}