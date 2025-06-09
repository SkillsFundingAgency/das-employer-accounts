using SFA.DAS.EmployerAccounts.Data.Contracts;
using System.Threading;

namespace SFA.DAS.EmployerAccounts.Queries.GetAllAccountLegalEntitiesByHashedAccountId
{
    public class GetAllAccountLegalEntitiesByHashedAccountIdQueryHandler(IAccountLegalEntityRepository accountLegalEntityRepository): IRequestHandler<GetAllAccountLegalEntitiesByHashedAccountIdQuery, GetAllAccountLegalEntitiesByHashedAccountIdQueryResult>
    {
        public async Task<GetAllAccountLegalEntitiesByHashedAccountIdQueryResult> Handle(GetAllAccountLegalEntitiesByHashedAccountIdQuery request, CancellationToken cancellationToken)
        {
            var accountLegalEntities = await accountLegalEntityRepository.GetAccountLegalEntities(
                request.SearchTerm,
                request.AccountIds,
                request.PageNumber,
                request.PageSize,
                request.SortColumn,
                request.IsAscending,
                cancellationToken);

            return new GetAllAccountLegalEntitiesByHashedAccountIdQueryResult
            {
                LegalEntities = accountLegalEntities
            };
        }
    }
}
