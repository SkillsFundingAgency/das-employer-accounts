namespace SFA.DAS.EmployerAccounts.Commands.SendNotification;

public class SendNotificationCommand : IRequest
{
    public string TemplateId { get; set; }
    public string RecipientsAddress { get; set; }
    public Dictionary<string, string> Tokens { get; set; }
}