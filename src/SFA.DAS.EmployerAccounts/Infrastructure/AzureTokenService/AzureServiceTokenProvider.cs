using Azure.Core;
using SFA.DAS.EmployerAccounts.Extensions;

namespace SFA.DAS.EmployerAccounts.Infrastructure.AzureTokenService;

public class AzureServiceTokenProvider : IAzureServiceTokenProvider
{
    public async Task<string> GetTokenAsync(string resourceIdentifier)
    {
        var azureServiceTokenProvider = ChainedTokenCredentialHelper.Create();

        var accessToken = (await azureServiceTokenProvider.GetTokenAsync(new TokenRequestContext(scopes: new string[] { resourceIdentifier }))).Token;

        return accessToken;
    }
}