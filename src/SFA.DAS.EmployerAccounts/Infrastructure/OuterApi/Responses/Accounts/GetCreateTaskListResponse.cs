namespace SFA.DAS.EmployerAccounts.Infrastructure.OuterApi.Responses.Accounts;

public record GetCreateTaskListResponse
{
    public string HashedAccountId { get; set; }
    public bool HasPayeScheme { get; set; }
    public bool NameConfirmed { get;  set; }
    public long? PendingAgreementId { get; set; }
    public bool AgreementAcknowledged { get; set; }
    public bool AddTrainingProviderAcknowledged { get; set; }
    public bool HasSignedAgreement { get; set; }
    public bool HasProviders { get; set; }
    public bool HasProviderPermissions { get; set; }
    public string UserFirstName { get; set; }
    public string UserLastName { get; set; }
}