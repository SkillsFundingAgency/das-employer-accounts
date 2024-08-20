using System.Threading;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Requests.UserAccounts;
using SFA.DAS.EmployerAccounts.Interfaces.OuterApi;

namespace SFA.DAS.EmployerAccounts.Commands.AddProviderDetailsFromInvitation
{
    public class AddProviderDetailsFromInvitationCommandHandler : IRequestHandler<AddProviderDetailsFromInvitationCommand, Unit>
    {
        private readonly IOuterApiClient _outerApiClient;
        private readonly ILogger<AddProviderDetailsFromInvitationCommandHandler> _logger;

        public AddProviderDetailsFromInvitationCommandHandler(ILogger<AddProviderDetailsFromInvitationCommandHandler> logger, IOuterApiClient outerApiClient)
        {
            _logger = logger;
            _outerApiClient = outerApiClient;
        }

        public async Task<Unit> Handle(AddProviderDetailsFromInvitationCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Add Provider Detail from Invitation with CorrelationId '{Id}'", request.CorrelationId);

            await _outerApiClient.Post(new AddProviderDetailsPostRequest(request.UserId, request.CorrelationId, request.AccountId,
                                                                            request.Email, request.FirstName, request.LastName));

            return Unit.Value;
        }
    }
}