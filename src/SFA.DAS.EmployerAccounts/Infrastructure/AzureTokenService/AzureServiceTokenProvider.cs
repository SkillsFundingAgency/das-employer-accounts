using Azure.Core;
using Azure.Identity;

namespace SFA.DAS.EmployerAccounts.Infrastructure.AzureTokenService;

public class AzureServiceTokenProvider : IAzureServiceTokenProvider
{
    public async Task<string> GetTokenAsync(string resourceIdentifier)
    {
        var azureServiceTokenProvider = new ChainedTokenCredential(
            new ManagedIdentityCredential(),
            new AzureCliCredential(),
            new VisualStudioCodeCredential(),
            new VisualStudioCredential()
        );
        
        var accessToken = (await azureServiceTokenProvider.GetTokenAsync(new TokenRequestContext(scopes: new string[] { resourceIdentifier }))).Token;

        return accessToken;
    }
}