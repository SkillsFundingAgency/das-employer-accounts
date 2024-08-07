using Azure.Core;
using Azure.Identity;

namespace SFA.DAS.EmployerAccounts.Infrastructure.AzureTokenService;

public class AzureServiceTokenProvider : IAzureServiceTokenProvider
{
    private readonly ChainedTokenCredential _azureServiceTokenProvider = new(
        new ManagedIdentityCredential(),
        new AzureCliCredential(),
        new VisualStudioCodeCredential(),
        new VisualStudioCredential()
    );

    public async Task<string> GetTokenAsync(string resourceIdentifier)
    {
        return (await _azureServiceTokenProvider.GetTokenAsync(new TokenRequestContext(scopes: [resourceIdentifier]))).Token;
    }
}