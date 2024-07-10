using System.Threading;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Queries.GetMemberById;

public record GetMemberByIdRequest : IRequest<GetMemberByIdResponse>
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
    : IRequestHandler<GetMemberByIdRequest, GetMemberByIdResponse>
{
    public async Task<GetMemberByIdResponse> Handle(GetMemberByIdRequest request, CancellationToken cancellationToken)
    {
        var hashedAccountId = encodingService.Encode(request.AccountId, EncodingType.AccountId);
        var member = await accountTeamRepository.GetMember(hashedAccountId, request.Id, request.MemberType) ?? new TeamMember();
        
        member.HashedId = encodingService.Encode(member.Id, EncodingType.AccountId);
        member.HashedAccountId = hashedAccountId;

        return new GetMemberByIdResponse
        {
            TeamMember = member
        };
    }
}