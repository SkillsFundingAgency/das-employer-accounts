using System.Diagnostics.CodeAnalysis;
using Azure.Identity;

namespace SFA.DAS.EmployerAccounts.Extensions;

[ExcludeFromCodeCoverage]
public static class ChainedTokenCredentialHelper
{
    public static ChainedTokenCredential Create()
    {
#if DEBUG
        return new ChainedTokenCredential(
           new AzureCliCredential(),
           new VisualStudioCodeCredential(),
           new VisualStudioCredential());
#else
        return new ChainedTokenCredential(
           new ManagedIdentityCredential(),
           new AzureCliCredential(),
           new VisualStudioCodeCredential(),
           new VisualStudioCredential());
#endif
    }
}
