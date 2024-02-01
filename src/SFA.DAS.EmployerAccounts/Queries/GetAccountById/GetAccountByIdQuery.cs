namespace SFA.DAS.EmployerAccounts.Queries.GetAccountById
{
    public class GetAccountByIdQuery : IRequest<GetAccountByIdResponse>
    {
        public long AccountId { get; set; }
    }
}