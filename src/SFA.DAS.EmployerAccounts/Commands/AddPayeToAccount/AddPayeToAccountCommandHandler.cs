using System.Threading;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerAccounts.Audit.Types;
using SFA.DAS.EmployerAccounts.Commands.AccountLevyStatus;
using SFA.DAS.EmployerAccounts.Commands.AuditCommand;
using SFA.DAS.EmployerAccounts.Data.Contracts;
using SFA.DAS.EmployerAccounts.Models.PAYE;
using SFA.DAS.EmployerAccounts.Queries.GetUserByRef;
using SFA.DAS.Encoding;
using SFA.DAS.NServiceBus.Services;

namespace SFA.DAS.EmployerAccounts.Commands.AddPayeToAccount;

public class AddPayeToAccountCommandHandler(
    IValidator<AddPayeToAccountCommand> validator,
    IPayeRepository payeRepository,
    IEventPublisher eventPublisher,
    IEncodingService encodingService,
    IMediator mediator)
    : IRequestHandler<AddPayeToAccountCommand>
{
    public async Task Handle(AddPayeToAccountCommand message, CancellationToken cancellationToken)
    {
        await ValidateMessage(message);

        var accountId = encodingService.Decode(message.HashedAccountId, EncodingType.AccountId);

        await payeRepository.AddPayeToAccount(
            new Paye
            {
                AccessToken = message.AccessToken,
                RefreshToken = message.RefreshToken,
                AccountId = accountId,
                EmpRef = message.Empref,
                RefName = message.EmprefName,
                Aorn = message.Aorn
            }
        );

        var userResponse = await mediator.Send(new GetUserByRefQuery { UserRef = message.ExternalUserId }, cancellationToken);

        await AddAuditEntry(message, accountId);

        await AddPayeScheme(message.Empref, accountId, userResponse.User.FullName, userResponse.User.UserRef, message.Aorn, message.EmprefName, userResponse.User.CorrelationId);
    }

    private async Task ValidateMessage(AddPayeToAccountCommand message)
    {
        var result = await validator.ValidateAsync(message);

        if (result.IsUnauthorized)
        {
            throw new UnauthorizedAccessException();
        }

        if (!result.IsValid())
        {
            throw new InvalidRequestException(result.ValidationDictionary);
        }
    }
    
    private async Task AddPayeScheme(string payeRef, long accountId, string userName, string userRef, string aorn, string schemeName, string correlationId)
    {
        await eventPublisher.Publish(new AddedPayeSchemeEvent
        {
            PayeRef = payeRef,
            AccountId = accountId,
            UserName = userName,
            UserRef = Guid.Parse(userRef),
            Created = DateTime.UtcNow,
            Aorn = aorn,
            SchemeName = schemeName,
            CorrelationId = correlationId
        });

        if (!string.IsNullOrWhiteSpace(aorn))
        {
            await mediator.Send(new AccountLevyStatusCommand
            {
                AccountId = accountId,
                ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy
            });
        }
    }

    private async Task AddAuditEntry(AddPayeToAccountCommand message, long accountId)
    {
        await mediator.Send(new CreateAuditCommand
        {
            EasAuditMessage = new AuditMessage
            {
                Category = "CREATED",
                Description = $"Paye scheme {message.Empref} added to account {accountId}",
                ChangedProperties = new List<PropertyUpdate>
                {
                    PropertyUpdate.FromString("Ref", message.Empref),
                    PropertyUpdate.FromString("AccessToken", message.AccessToken),
                    PropertyUpdate.FromString("RefreshToken", message.RefreshToken),
                    PropertyUpdate.FromString("Name", message.EmprefName),
                    PropertyUpdate.FromString("Aorn", message.Aorn)
                },
                RelatedEntities = new List<AuditEntity> { new AuditEntity { Id = accountId.ToString(), Type = "Account" } },
                AffectedEntity = new AuditEntity { Type = "Paye", Id = message.Empref }
            }
        });
    }
}