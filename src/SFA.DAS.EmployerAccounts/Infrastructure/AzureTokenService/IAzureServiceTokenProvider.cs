namespace SFA.DAS.EmployerAccounts.Infrastructure.AzureTokenService;

public interface IAzureServiceTokenProvider
{
    Task<string> GetTokenAsync(string resourceIdentifier);
}