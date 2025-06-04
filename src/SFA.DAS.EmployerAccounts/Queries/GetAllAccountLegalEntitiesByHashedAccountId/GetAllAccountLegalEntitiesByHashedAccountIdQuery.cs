namespace SFA.DAS.EmployerAccounts.Queries.GetAllAccountLegalEntitiesByHashedAccountId
{
    public sealed record GetAllAccountLegalEntitiesByHashedAccountIdQuery(
        string SearchTerm,
        List<long> AccountIds,
        int PageNumber,
        int PageSize,
        string SortColumn,
        bool IsAscending)
        : IRequest<GetAllAccountLegalEntitiesByHashedAccountIdQueryResult>;
}