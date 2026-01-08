using SFA.DAS.EmployerAccounts.Configuration;

namespace SFA.DAS.EmployerAccounts.Services;

public class ContentApiClientWithCaching(
    IContentApiClient contentService,
    ICacheStorageService cacheStorageService,
    EmployerAccountsConfiguration employerAccountsConfiguration)
    : IContentApiClient
{
    public async Task<string> Get(string type, string applicationId)
    {
        var cacheKey = $"{applicationId}_{type}".ToLowerInvariant();

        try
        {
            var (success, cachedContentBanner) = await cacheStorageService.TryGetAsync(cacheKey);
            if (success && cachedContentBanner != null)
            {
                return cachedContentBanner;
            }

            var content = await contentService.Get(type, applicationId);

            if (content != null)
            {
                await cacheStorageService.Save(cacheKey, content, employerAccountsConfiguration.DefaultCacheExpirationInMinutes);
            }

            return content;
        }
        catch(Exception ex)
        {
            throw new ArgumentException($"Failed to get content for {cacheKey}", ex);
        }
    }
}