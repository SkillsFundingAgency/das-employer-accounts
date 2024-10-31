namespace SFA.DAS.EmployerAccounts.Commands.CreateAccount;

public class CreateAccountCommandResponse
{
    public string HashedAccountId { get; set; }
    public string HashedAgreementId { get; set; }
    public long AccountId { get; internal set; }
    public User User { get; set; }
}