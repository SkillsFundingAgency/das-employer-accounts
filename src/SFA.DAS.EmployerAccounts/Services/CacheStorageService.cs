using SFA.DAS.Caches;
using Newtonsoft.Json;
using SFA.DAS.EmployerAccounts.Configuration;
namespace SFA.DAS.EmployerAccounts.Services;

public class CacheStorageService : ICacheStorageService
{
    private readonly IDistributedCache _distributedCache;
    private readonly EmployerAccountsConfiguration _config;

    public CacheStorageService(IDistributedCache distributedCache, EmployerAccountsConfiguration config)
    {
        _distributedCache = distributedCache;
        _config = config;
    }

    public async Task Save<T>(string key, T item, int expirationInMinutes)
    {
        var json = JsonConvert.SerializeObject(item);
        await _distributedCache.SetCustomValueAsync(key, json, TimeSpan.FromMinutes(_config.DefaultCacheExpirationInMinutes));
    }

    public async Task<(bool, string)> TryGetAsync(string key)
    {
        var value = await Get<string>(key);
        return (value != null, value);
    }

    private async Task<T> Get<T>(string key)
    {
        var json = string.Empty;
        if (await _distributedCache.ExistsAsync(key))
        {
            json = await _distributedCache.GetCustomValueAsync<string>(key);
        }
        return JsonConvert.DeserializeObject<T>(json);
    }

    public async Task Delete(string key)
    {
        await _distributedCache.RemoveFromCache(key);
    }
}