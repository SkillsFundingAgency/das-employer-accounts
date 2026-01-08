using SFA.DAS.Caches;
using Newtonsoft.Json;
using SFA.DAS.EmployerAccounts.Configuration;

namespace SFA.DAS.EmployerAccounts.Services;

public class CacheStorageService(IDistributedCache distributedCache, EmployerAccountsConfiguration config)
    : ICacheStorageService
{
    public async Task Save<T>(string key, T item, int expirationInMinutes)
    {
        var json = JsonConvert.SerializeObject(item);
        await distributedCache.SetCustomValueAsync(key, json, TimeSpan.FromMinutes(config.DefaultCacheExpirationInMinutes));
    }

    public async Task<(bool Success, string Value)> TryGetAsync(string key)
    {
        if (!await distributedCache.ExistsAsync(key))
        {
            return (false, null);
        }

        var json = await distributedCache.GetCustomValueAsync<string>(key);
        var value = JsonConvert.DeserializeObject<string>(json);
        
        return (value != null, value);
    }

    public async Task Delete(string key)
    {
        await distributedCache.RemoveFromCache(key);
    }
}