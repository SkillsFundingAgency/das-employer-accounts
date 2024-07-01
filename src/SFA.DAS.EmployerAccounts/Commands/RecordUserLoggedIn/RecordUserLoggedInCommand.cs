namespace SFA.DAS.EmployerAccounts.Commands.RecordUserLoggedIn;

public class RecordUserLoggedInCommand : IRequest
{
    public string UserRef { get; set; }
}