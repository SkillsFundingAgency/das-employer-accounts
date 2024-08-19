using System.Threading;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Queries.GetMember;

public class GetMemberQueryHandler : IRequestHandler<GetMemberRequest, GetMemberResponse>
{
    private readonly IEmployerAccountTeamRepository _accountTeamRepository;
    private readonly IEncodingService _encodingService;

    public GetMemberQueryHandler(IEmployerAccountTeamRepository accountTeamRepository, IEncodingService encodingService)
    {
        _accountTeamRepository = accountTeamRepository ?? throw new ArgumentNullException(nameof(accountTeamRepository));
        _encodingService = encodingService;
    }

    public async Task<GetMemberResponse> Handle(GetMemberRequest message, CancellationToken cancellationToken)
    {
        var hashedAccountId = _encodingService.Encode(message.AccountId, EncodingType.AccountId);

        TeamMember member;

        if (!string.IsNullOrEmpty(message.HashedUserId))
        {
            var userId = _encodingService.Decode(message.HashedUserId, EncodingType.AccountId);
            member = await _accountTeamRepository.GetMember(hashedAccountId, userId, message.IsUser) ?? new TeamMember();
        }
        else
        {
            member = await _accountTeamRepository.GetMember(hashedAccountId, message.Email, message.OnlyIfMemberIsActive) ?? new TeamMember();
        }

        member.HashedUserId = _encodingService.Encode(member.Id, EncodingType.AccountId);
        member.HashedAccountId = hashedAccountId;

        return new GetMemberResponse
        {
            TeamMember = member
        };
    }
}