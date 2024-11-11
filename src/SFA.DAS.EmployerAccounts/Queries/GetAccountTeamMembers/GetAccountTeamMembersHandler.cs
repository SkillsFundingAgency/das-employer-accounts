using System.Threading;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Queries.GetAccountTeamMembers;

public class GetAccountTeamMembersHandler(
    IValidator<GetAccountTeamMembersQuery> validator,
    IEmployerAccountTeamRepository repository,
    IEncodingService encodingService)
    : IRequestHandler<GetAccountTeamMembersQuery, GetAccountTeamMembersResponse>
{
    public async Task<GetAccountTeamMembersResponse> Handle(GetAccountTeamMembersQuery message, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(message);

        if (!validationResult.IsValid())
        {
            if (validationResult.IsUnauthorized)
            {
                throw new UnauthorizedAccessException("User not authorised");
            }

            throw new InvalidRequestException(validationResult.ValidationDictionary);
        }

        var teamMembers = await repository.GetAccountTeamMembersForUserId(message.HashedAccountId, message.ExternalUserId);

        if (teamMembers != null && teamMembers.Any())
        {
            foreach (var teamMember in teamMembers)
            {
                teamMember.HashedUserId = encodingService.Encode(teamMember.Id, EncodingType.AccountId);
            }
        }

        return new GetAccountTeamMembersResponse { TeamMembers = teamMembers };
    }
}