using System.Threading;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Queries.GetMemberById;

public record GetMemberByIdQuery : IRequest<GetMemberByIdResponse>
{
    public long Id { get; set; }
    public long AccountId { get; set; }
    public MemberType MemberType { get; set; }
}

public record GetMemberByIdResponse
{
    public TeamMember TeamMember { get; set; }
}

public class GetMemberByIdQueryHandler(
    IEmployerAccountTeamRepository accountTeamRepository,
    IEncodingService encodingService)
    : IRequestHandler<GetMemberByIdQuery, GetMemberByIdResponse>
{
    public async Task<GetMemberByIdResponse> Handle(GetMemberByIdQuery query, CancellationToken cancellationToken)
    {
        var hashedAccountId = encodingService.Encode(query.AccountId, EncodingType.AccountId);
        var member = await accountTeamRepository.GetMember(hashedAccountId, query.Id, query.MemberType) ?? new TeamMember();
        
        member.HashedUserId = encodingService.Encode(member.Id, EncodingType.AccountId);
        member.HashedAccountId = hashedAccountId;

        return new GetMemberByIdResponse
        {
            TeamMember = member
        };
    }
}