namespace SFA.DAS.EmployerAccounts.Commands.SendEmail;

public class SendEmailCommand : IRequest
{
    public string TemplateId { get; }
    public string RecipientsAddress { get; }
    public IReadOnlyDictionary<string, string> Tokens { get; }

    public SendEmailCommand(string templateId, string recipientsAddress, IReadOnlyDictionary<string, string> tokens)
    {
        TemplateId = templateId;
        RecipientsAddress = recipientsAddress;
        Tokens = tokens;
    }
}