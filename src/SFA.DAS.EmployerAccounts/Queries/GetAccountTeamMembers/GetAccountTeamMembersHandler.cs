using System.Threading;
using SFA.DAS.EmployerAccounts.Audit.Types;
using SFA.DAS.EmployerAccounts.Commands.AuditCommand;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Extensions;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Queries.GetAccountTeamMembers;

public class GetAccountTeamMembersHandler(
    IValidator<GetAccountTeamMembersQuery> validator,
    IEmployerAccountTeamRepository repository,
    IMediator mediator,
    IMembershipRepository membershipRepository,
    IUserContext userContext,
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

        foreach (var teamMember in teamMembers)
        {
            teamMember.HashedUserId = encodingService.Encode(teamMember.Id, EncodingType.AccountId);
        }

        if (userContext.IsSupportConsoleUser())
        {
            await AuditAccess(message);
        }

        return new GetAccountTeamMembersResponse { TeamMembers = teamMembers };
    }

    private async Task AuditAccess(GetAccountTeamMembersQuery message)
    {
        var caller = await membershipRepository.GetCaller(message.HashedAccountId, message.ExternalUserId);

        await mediator.Send(new CreateAuditCommand
        {
            EasAuditMessage = new AuditMessage
            {
                Category = "VIEW",
                Description = $"Account {caller.AccountId} team members viewed",
                AffectedEntity = new AuditEntity { Type = "TeamMembers", Id = caller.AccountId.ToString() }
            }
        });
    }
}