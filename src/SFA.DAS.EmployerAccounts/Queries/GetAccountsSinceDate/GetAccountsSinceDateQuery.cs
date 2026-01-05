namespace SFA.DAS.EmployerAccounts.Queries.GetAccountsSinceDate;

public class GetAccountsSinceDateQuery: IRequest<GetAccountsSinceDateResponse>
{
    public DateTime? SinceDate { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
