namespace SFA.DAS.EmployerAccounts.Queries.GetPayeAccountByRef;

public class GetPayeAccountByRefQuery : IRequest<GetPayeAccountByRefResponse>
{
    public string Ref { get; set; }
}