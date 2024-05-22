using System.Threading;
using NServiceBus;

namespace SFA.DAS.EmployerAccounts.Commands.SendEmail;

public class SendEmailCommandHandler : IRequestHandler<SendEmailCommand>
{
    private readonly IMessageSession _messageSession;

    public SendEmailCommandHandler(IMessageSession messageSession) => _messageSession = messageSession;

    public async Task Handle(SendEmailCommand request, CancellationToken cancellationToken) => await _messageSession.Send(request);
}