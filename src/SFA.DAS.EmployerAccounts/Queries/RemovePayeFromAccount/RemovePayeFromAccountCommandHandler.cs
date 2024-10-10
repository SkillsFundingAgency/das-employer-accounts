using System.Threading;
using SFA.DAS.EmployerAccounts.Attributes;
using SFA.DAS.EmployerAccounts.Audit.Types;
using SFA.DAS.EmployerAccounts.Commands.AuditCommand;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.Encoding;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerAccounts.Queries.RemovePayeFromAccount;

[method: ServiceBusConnectionKey("employer_shared")]
public class RemovePayeFromAccountCommandHandler(
    IMediator mediator,
    IValidator<RemovePayeFromAccountCommand> validator,
    IPayeRepository payeRepository,
    IEncodingService encodingService,
    IEventPublisher eventPublisher,
    IMembershipRepository membershipRepository)
    : IRequestHandler<RemovePayeFromAccountCommand>
{
    public async Task Handle(RemovePayeFromAccountCommand message, CancellationToken cancellationToken)
    {
        await ValidateMessage(message);

        var accountId = encodingService.Decode(message.HashedAccountId, EncodingType.AccountId);

        await AddAuditEntry(message.UserId, message.PayeRef, accountId.ToString());

        await payeRepository.RemovePayeFromAccount(accountId, message.PayeRef);

        var loggedInPerson = await membershipRepository.GetCaller(accountId, message.UserId);

        await QueuePayeRemovedMessage(message.PayeRef, accountId, message.CompanyName, loggedInPerson.FullName(), loggedInPerson.UserRef);
    }

    private async Task ValidateMessage(RemovePayeFromAccountCommand message)
    {
        var result = await validator.ValidateAsync(message);

        if (!result.IsValid())
        {
            throw new InvalidRequestException(result.ValidationDictionary);
        }

        if (result.IsUnauthorized)
        {
            throw new UnauthorizedAccessException();
        }
    }
    
    private Task QueuePayeRemovedMessage(string payeRef, long accountId, string organisationName, string userName, Guid userRef)
    {
        return eventPublisher.Publish(new DeletedPayeSchemeEvent
        {
            AccountId = accountId,
            PayeRef = payeRef,
            OrganisationName = organisationName,
            UserName = userName,
            UserRef = userRef,
            Created = DateTime.UtcNow
        });
    }

    private async Task AddAuditEntry(string userId, string payeRef, string accountId)
    {
        await mediator.Send(new CreateAuditCommand
        {
            EasAuditMessage = new AuditMessage
            {
                Category = "DELETED",
                Description = $"User {userId} has removed PAYE schema {payeRef} from account {accountId}",
                ChangedProperties = new List<PropertyUpdate>
                {
                    new PropertyUpdate {PropertyName = "AccountId", NewValue = accountId},
                    new PropertyUpdate {PropertyName = "UserId", NewValue = userId},
                    new PropertyUpdate {PropertyName = "PayeRef", NewValue = payeRef}
                },
                RelatedEntities = new List<AuditEntity> { new AuditEntity { Id = accountId, Type = "Account" } },
                AffectedEntity = new AuditEntity { Type = "Paye", Id = payeRef }
            }
        });
    }
}