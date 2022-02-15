using System.Threading.Tasks;

namespace E.S.ApiClientHandler.Interfaces
{
    public interface IApiCachingManager
    {
        Task<T> GetAsync<T>(string key) where T : class;
        Task SetAsync<T>(string key, T value, int? cacheTimeInSeconds = null) where T : class;
    }
}