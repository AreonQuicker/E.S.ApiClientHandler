using System.Threading.Tasks;
using E.S.ApiClientHandler.Interfaces;
using E.S.Simple.MemoryCache.Interfaces;

namespace E.S.ApiClientHandler.Managers
{
    public class ApiMemoryCachingManager : IApiCachingManager
    {
        private readonly IMemoryCacheManager _memoryCacheManager;

        public ApiMemoryCachingManager(IMemoryCacheManager memoryCacheManager)
        {
            _memoryCacheManager = memoryCacheManager;
        }

        public async Task<T> GetAsync<T>(string key) where T : class
        {
            return _memoryCacheManager.Get<T>(key);
        }

        public Task SetAsync<T>(string key, T value, int? cacheTimeInSeconds = null) where T : class
        {
            _memoryCacheManager.Set(key, value, cacheTimeInSeconds ?? 0);

            return Task.CompletedTask;
        }
    }
}