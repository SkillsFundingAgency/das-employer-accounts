using System.Threading;
using SFA.DAS.EmployerAccounts.Audit.Types;
using SFA.DAS.EmployerAccounts.Commands.AuditCommand;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.Encoding;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerAccounts.Commands.RenameEmployerAccount;

public class RenameEmployerAccountCommandHandler(
    IEventPublisher eventPublisher,
    IEmployerAccountRepository accountRepository,
    IMembershipRepository membershipRepository,
    IValidator<RenameEmployerAccountCommand> validator,
    IEncodingService encodingService,
    IMediator mediator)
    : IRequestHandler<RenameEmployerAccountCommand>
{
    public async Task Handle(RenameEmployerAccountCommand message, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(message);

        if (!validationResult.IsValid())
        {
            throw new InvalidRequestException(validationResult.ValidationDictionary);
        }

        if (validationResult.IsUnauthorized)
        {
            throw new UnauthorizedAccessException();
        }

        var accountId = encodingService.Decode(message.HashedAccountId, EncodingType.AccountId);

        var account = await accountRepository.GetAccountById(accountId);

        var accountPreviousName = account.Name;

        await accountRepository.RenameAccount(accountId, message.NewName);

        var owner = await membershipRepository.GetCaller(message.HashedAccountId, message.ExternalUserId);

        await AddAuditEntry(owner.Email, accountId, message.NewName);

        await PublishAccountRenamedMessage(accountId, accountPreviousName, message.NewName, owner.FullName(), owner.UserRef);
    }

    private Task PublishAccountRenamedMessage(
        long accountId, string previousName, string currentName, string creatorName, Guid creatorUserRef)
    {
        return eventPublisher.Publish(new ChangedAccountNameEvent
        {
            PreviousName = previousName,
            CurrentName = currentName,
            AccountId = accountId,
            Created = DateTime.UtcNow,
            UserName = creatorName,
            UserRef = creatorUserRef
        });
    }
    
    private async Task AddAuditEntry(string ownerEmail, long accountId, string name)
    {
        await mediator.Send(new CreateAuditCommand
        {
            EasAuditMessage = new AuditMessage
            {
                Category = "UPDATED",
                Description = $"User {ownerEmail} has renamed account {accountId} to {name}",
                ChangedProperties =
                [
                    new() { PropertyName = "AccountId", NewValue = accountId.ToString() },
                    new() { PropertyName = "Name", NewValue = name }
                ],
                RelatedEntities = [new() { Id = accountId.ToString(), Type = "Account" }],
                AffectedEntity = new AuditEntity { Type = "Account", Id = accountId.ToString() }
            }
        });
    }
}