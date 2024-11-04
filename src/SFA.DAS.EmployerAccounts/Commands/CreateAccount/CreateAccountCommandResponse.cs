namespace SFA.DAS.EmployerAccounts.Commands.CreateAccount;

public class CreateAccountCommandResponse
{
    public string HashedAccountId { get; set; }
    public string HashedAgreementId { get; set; }
    public long AccountId { get; set; }
    public User User { get; set; }
    public long AccountLegalEntityId { get; set; }
}