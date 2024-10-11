using System.Diagnostics.CodeAnalysis;
using Azure.Core;
using Azure.Identity;
using SFA.DAS.EmployerAccounts.Extensions;

namespace SFA.DAS.EmployerAccounts.Infrastructure.AzureTokenService;

[ExcludeFromCodeCoverage]
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