using SFA.DAS.EmployerAccounts.Api.Responses;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Models.Account;

namespace SFA.DAS.EmployerAccounts.Api.Extensions
{
    public static class PageInfoExtensions
    {
        public static PageInfo ToPageInfo(this PaginatedList<AccountLegalEntity> paginatedList)
            =>
                new(paginatedList.TotalCount,
                    paginatedList.PageIndex,
                    paginatedList.PageSize,
                    paginatedList.TotalPages,
                    paginatedList.HasPreviousPage,
                    paginatedList.HasNextPage);
    }
}
