namespace SFA.DAS.EmployerAccounts.Queries.GetPayeSchemeByRef;

public class GetPayeSchemeByRefQuery : IRequest<GetPayeSchemeByRefResponse>
{
    public long AccountId { get; set; }
    public string Ref { get; set; }
}