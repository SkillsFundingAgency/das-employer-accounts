namespace SFA.DAS.EmployerAccounts.Queries.GetAllAccountLegalEntitiesByHashedAccountId
{
    public sealed record GetAllAccountLegalEntitiesByHashedAccountIdQuery(
        long AccountId,
        int PageNumber,
        int PageSize,
        string SortColumn,
        bool IsAscending)
        : IRequest<GetAllAccountLegalEntitiesByHashedAccountIdQueryResult>;
}