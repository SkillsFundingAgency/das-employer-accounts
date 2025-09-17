using SFA.DAS.EmployerAccounts.Models.Account;

namespace SFA.DAS.EmployerAccounts.Queries.GetAccounts
{
    public class GetAccountsQuery: IRequest<GetAccountsResponse>
    {
        public DateTime? SinceDate { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
