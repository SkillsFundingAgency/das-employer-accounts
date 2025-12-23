namespace SFA.DAS.EmployerAccounts.Queries.GetVacancies;

public class GetVacanciesRequest : IRequest<GetVacanciesResponse>
{
    public long AccountId { get; set; }
}