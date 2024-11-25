using System.Diagnostics.CodeAnalysis;
using Azure.Core;
using Azure.Identity;
using SFA.DAS.EmployerAccounts.Extensions;

namespace SFA.DAS.EmployerAccounts.Infrastructure.AzureTokenService;

[ExcludeFromCodeCoverage]
public class AzureServiceTokenProvider : IAzureServiceTokenProvider
{
    private const int MaxRetries = 2;
    private static readonly TimeSpan NetworkTimeout = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(100);
    
    private readonly ChainedTokenCredential _azureServiceTokenProvider = new ChainedTokenCredential(
        new ManagedIdentityCredential(options: new TokenCredentialOptions
        {
            Retry = { NetworkTimeout = NetworkTimeout, MaxRetries = MaxRetries, Delay = Delay, Mode = RetryMode.Fixed }
        }),
        new AzureCliCredential(options: new AzureCliCredentialOptions
        {
            Retry = { NetworkTimeout = NetworkTimeout, MaxRetries = MaxRetries, Delay = Delay, Mode = RetryMode.Fixed }
        }),
        new VisualStudioCredential(options: new VisualStudioCredentialOptions
        {
            Retry = { NetworkTimeout = NetworkTimeout, MaxRetries = MaxRetries, Delay = Delay, Mode = RetryMode.Fixed }
        }),
        new VisualStudioCodeCredential(options: new VisualStudioCodeCredentialOptions
        {
            Retry = { NetworkTimeout = NetworkTimeout, MaxRetries = MaxRetries, Delay = Delay, Mode = RetryMode.Fixed }
        }));

    public async Task<string> GetTokenAsync(string resourceIdentifier)
    {
        return (await _azureServiceTokenProvider.GetTokenAsync(new TokenRequestContext(scopes: [resourceIdentifier]))).Token;
    }
}