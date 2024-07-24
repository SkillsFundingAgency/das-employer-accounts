using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.EmployerAccounts.Queries.GetEmployerAccountDetail;

public class GetEmployerAccountDetailByIdQuery : IRequest<GetEmployerAccountDetailByIdResponse>
{
    [Required]
    public long AccountId { get; set; }
}