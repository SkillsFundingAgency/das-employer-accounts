namespace SFA.DAS.EmployerAccounts.Queries.GetContent;

public class GetContentRequest : IRequest<GetContentResponse>
{
    public string ContentType { get; set; }
}