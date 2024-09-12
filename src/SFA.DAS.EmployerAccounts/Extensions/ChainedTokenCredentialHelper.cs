using Azure.Identity;

namespace SFA.DAS.EmployerAccounts.Extensions
{
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
}
