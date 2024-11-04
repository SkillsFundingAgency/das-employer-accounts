using System.Threading;
using SFA.DAS.EmployerAccounts.Audit.Types;
using SFA.DAS.EmployerAccounts.Commands.AuditCommand;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Queries.GetUserByRef;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerAccounts.Commands.CreateUserAccount;

public class CreateUserAccountCommandHandler(
    IAccountRepository accountRepository,
    IMediator mediator,
    IValidator<CreateUserAccountCommand> validator,
    IEncodingService encodingService)
    : IRequestHandler<CreateUserAccountCommand, CreateUserAccountCommandResponse>
{
    public async Task<CreateUserAccountCommandResponse> Handle(CreateUserAccountCommand message, CancellationToken cancellationToken)
    {
        ValidateMessage(message);

        var userResponse = await mediator.Send(new GetUserByRefQuery { UserRef = message.ExternalUserId }, cancellationToken);

        var createAccountResult = await accountRepository.CreateUserAccount(userResponse.User.Id, message.OrganisationName);

        var hashedAccountId = encodingService.Encode(createAccountResult.AccountId, EncodingType.AccountId);
        var publicHashedAccountId = encodingService.Encode(createAccountResult.AccountId, EncodingType.PublicAccountId);

        await accountRepository.UpdateAccountHashedIds(createAccountResult.AccountId, hashedAccountId, publicHashedAccountId);
        
        await CreateAuditEntries(message, createAccountResult, hashedAccountId, userResponse.User);

        return new CreateUserAccountCommandResponse
        {
            HashedAccountId = hashedAccountId
        };
    }
    
    private void ValidateMessage(CreateUserAccountCommand message)
    {
        var validationResult = validator.Validate(message);

        if (!validationResult.IsValid())
            throw new InvalidRequestException(validationResult.ValidationDictionary);
    }

    private async Task CreateAuditEntries(CreateUserAccountCommand message, CreateUserAccountResult returnValue,
        string hashedAccountId, User user)
    {
        //Account
        await mediator.Send(new CreateAuditCommand
        {
            EasAuditMessage = new AuditMessage
            {
                Category = "CREATED",
                Description = $"Account {message.OrganisationName} created with id {returnValue.AccountId}",
                ChangedProperties = new List<PropertyUpdate>
                {
                    PropertyUpdate.FromLong("AccountId", returnValue.AccountId),
                    PropertyUpdate.FromString("HashedId", hashedAccountId),
                    PropertyUpdate.FromString("Name", message.OrganisationName),
                    PropertyUpdate.FromDateTime("CreatedDate", DateTime.UtcNow),
                },
                AffectedEntity = new AuditEntity { Type = "Account", Id = returnValue.AccountId.ToString() },
                RelatedEntities = new List<AuditEntity>()
            }
        });

        //Membership Account
        await mediator.Send(new CreateAuditCommand
        {
            EasAuditMessage = new AuditMessage
            {
                Category = "CREATED",
                Description = $"User {message.ExternalUserId} added to account {returnValue.AccountId} as owner",
                ChangedProperties = new List<PropertyUpdate>
                {
                    PropertyUpdate.FromLong("AccountId", returnValue.AccountId),
                    PropertyUpdate.FromString("UserId", message.ExternalUserId),
                    PropertyUpdate.FromString("Role", Role.Owner.ToString()),
                    PropertyUpdate.FromDateTime("CreatedDate", DateTime.UtcNow)
                },
                RelatedEntities = new List<AuditEntity>
                {
                    new AuditEntity { Id = returnValue.AccountId.ToString(), Type = "Account" },
                    new AuditEntity { Id = user.Id.ToString(), Type = "User" }
                },
                AffectedEntity = new AuditEntity { Type = "Membership", Id = message.ExternalUserId }
            }
        });
    }
}