using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using E.S.ApiClientHandler.Config;
using E.S.ApiClientHandler.Interfaces;
using E.S.ApiClientHandler.Models;
using Microsoft.Extensions.Logging;

namespace E.S.ApiClientHandler.Core
{
    public abstract class ApiIntegrationBase : IDisposable
    {
        protected readonly int AbsoluteExpirationRelativeToNowInSeconds;
        protected readonly IApiRequestBuilder ApiRequestBuilder;
        protected readonly IApiCachingManager CachingManager;
        protected readonly HttpClient Client;

        protected ApiIntegrationBase(string apiBaseUrl, string authorizationHeader = null, string userName = null,
            ILogger logger = null, IApiCachingManager cachingManager = null,
            int absoluteExpirationRelativeToNowInSeconds = 600)
        {
            if (apiBaseUrl == null) throw new ArgumentNullException(nameof(apiBaseUrl));
            CachingManager = cachingManager;
            AbsoluteExpirationRelativeToNowInSeconds = absoluteExpirationRelativeToNowInSeconds;
            Client = new HttpClient {BaseAddress = new Uri(apiBaseUrl)};
            if (!string.IsNullOrEmpty(authorizationHeader))
                Client.DefaultRequestHeaders.Add("Authorization", authorizationHeader);
            ApiRequestBuilder = Core.ApiRequestBuilder.Make(Client, apiBaseUrl,
                ApiRequestBuilderConfig.Create(true, ApiRequestBuilderLoggerConfig.Create(logger, userName)));
        }

        protected ApiIntegrationBase(string apiBaseUrl) : this(apiBaseUrl, null, null, null, null)
        {
        }

        protected ApiIntegrationBase(string apiBaseUrl, string authorizationHeader = null, string userName = null) :
            this(
                apiBaseUrl, authorizationHeader, userName, null, null)
        {
        }

        protected ApiIntegrationBase(string apiBaseUrl, string authorizationHeader = null, string userName = null,
            ILogger logger = null) : this(apiBaseUrl, authorizationHeader, userName, logger, null)
        {
        }

        #region IDisposable

        public void Dispose()
        {
            ApiRequestBuilder.Dispose();
            Client.Dispose();
        }

        #endregion

        #region IIntegrationBase

        public async Task<T> GetAsync<T>(string path, bool withCache = false) where T : class, new()
        {
            var newUrl = path.Replace(Client?.BaseAddress?.ToString() ?? "", "");
            var result = await ApiRequestBuilder.New()
                .WithMethod(HttpMethod.Get)
                .WithCacheClient(withCache ? CachingManager : null, AbsoluteExpirationRelativeToNowInSeconds)
                .WithUrl(newUrl)
                .ExecuteAsync<ContentResponse<T>>();
            return GetData(result);
        }

        public T Get<T>(string path, bool withCache = false) where T : class, new()
        {
            var newUrl = path.Replace(Client?.BaseAddress?.ToString() ?? "", "");
            var result = ApiRequestBuilder.New()
                .WithMethod(HttpMethod.Get)
                .WithCacheClient(withCache ? CachingManager : null, AbsoluteExpirationRelativeToNowInSeconds)
                .WithUrl(newUrl)
                .ExecuteAsync<ContentResponse<T>>()
                .GetAwaiter()
                .GetResult();
            return GetData(result);
        }

        public async Task<T> GetAsIsAsync<T>(string path, bool withCache = false) where T : class, new()
        {
            var newUrl = path.Replace(Client?.BaseAddress?.ToString() ?? "", "");
            var result = await ApiRequestBuilder.New()
                .WithMethod(HttpMethod.Get)
                .WithCacheClient(withCache ? CachingManager : null, AbsoluteExpirationRelativeToNowInSeconds)
                .WithUrl(newUrl)
                .ExecuteAsync<T>();
            return GetData(result);
        }

        public T GetAsIs<T>(string path, bool withCache = false) where T : class, new()
        {
            var newUrl = path.Replace(Client?.BaseAddress?.ToString() ?? "", "");
            var result = ApiRequestBuilder.New()
                .WithMethod(HttpMethod.Get)
                .WithCacheClient(withCache ? CachingManager : null, AbsoluteExpirationRelativeToNowInSeconds)
                .WithUrl(newUrl)
                .ExecuteAsync<T>()
                .GetAwaiter()
                .GetResult();
            return GetData(result);
        }

        public async Task<List<T>> GetListAsync<T>(string path, bool withCache = false) where T : class
        {
            var newUrl = path.Replace(Client?.BaseAddress?.ToString() ?? "", "");
            var result = await ApiRequestBuilder.New()
                .WithMethod(HttpMethod.Get)
                .WithCacheClient(withCache ? CachingManager : null, AbsoluteExpirationRelativeToNowInSeconds)
                .WithUrl(newUrl)
                .ExecuteAsync<ContentResponse<List<T>>>();
            return (result?.Value?.Data ?? new List<T>()).Where(w => w != null).ToList();
        }

        public List<T> GetList<T>(string path, bool withCache = false) where T : class
        {
            var newUrl = path.Replace(Client?.BaseAddress?.ToString() ?? "", "");
            var result = ApiRequestBuilder.New()
                .WithMethod(HttpMethod.Get)
                .WithCacheClient(withCache ? CachingManager : null, AbsoluteExpirationRelativeToNowInSeconds)
                .WithUrl(newUrl)
                .ExecuteAsync<ContentResponse<List<T>>>()
                .GetAwaiter()
                .GetResult();
            return (result?.Value?.Data ?? new List<T>()).Where(w => w != null).ToList();
        }

        public async Task<List<T>> GetAsIsListAsync<T>(string path, bool withCache = false) where T : class
        {
            var newUrl = path.Replace(Client?.BaseAddress?.ToString() ?? "", "");
            var result = await ApiRequestBuilder.New()
                .WithMethod(HttpMethod.Get)
                .WithCacheClient(withCache ? CachingManager : null, AbsoluteExpirationRelativeToNowInSeconds)
                .WithUrl(newUrl)
                .ExecuteAsync<List<T>>();
            return (result?.Value ?? new List<T>()).Where(w => w != null).ToList();
        }

        public List<T> GetAsIsList<T>(string path, bool withCache = false) where T : class
        {
            var newUrl = path.Replace(Client?.BaseAddress?.ToString() ?? "", "");
            var result = ApiRequestBuilder.New()
                .WithMethod(HttpMethod.Get)
                .WithCacheClient(withCache ? CachingManager : null, AbsoluteExpirationRelativeToNowInSeconds)
                .WithUrl(newUrl)
                .ExecuteAsync<List<T>>()
                .GetAwaiter()
                .GetResult();
            return (result?.Value ?? new List<T>()).Where(w => w != null).ToList();
        }

        public async Task<T> PostAsync<T>(string path, object content) where T : class, new()
        {
            var newUrl = path.Replace(Client?.BaseAddress?.ToString() ?? "", "");
            var result = await ApiRequestBuilder.New()
                .WithMethod(HttpMethod.Post)
                .WithContent(content)
                .WithUrl(newUrl)
                .ExecuteAsync<ContentResponse<T>>();
            return GetData(result);
        }

        public T Post<T>(string path, object content) where T : class, new()
        {
            var newUrl = path.Replace(Client?.BaseAddress?.ToString() ?? "", "");
            var result = ApiRequestBuilder.New()
                .WithMethod(HttpMethod.Post)
                .WithContent(content)
                .WithUrl(newUrl)
                .ExecuteAsync<ContentResponse<T>>()
                .GetAwaiter()
                .GetResult();
            return GetData(result);
        }

        public async Task<T> PostAsIsAsync<T>(string path, object content) where T : class, new()
        {
            var newUrl = path.Replace(Client?.BaseAddress?.ToString() ?? "", "");
            var result = await ApiRequestBuilder.New()
                .WithMethod(HttpMethod.Post)
                .WithContent(content)
                .WithUrl(newUrl)
                .ExecuteAsync<T>();
            return GetData(result);
        }

        public T PostAsIs<T>(string path, object content) where T : class, new()
        {
            var newUrl = path.Replace(Client?.BaseAddress?.ToString() ?? "", "");
            var result = ApiRequestBuilder.New()
                .WithMethod(HttpMethod.Post)
                .WithContent(content)
                .WithUrl(newUrl)
                .ExecuteAsync<T>()
                .GetAwaiter()
                .GetResult();
            return GetData(result);
        }

        public async Task<List<T>> PostListAsync<T>(string path, object content) where T : class
        {
            var newUrl = path.Replace(Client?.BaseAddress?.ToString() ?? "", "");
            var result = await ApiRequestBuilder.New()
                .WithMethod(HttpMethod.Post)
                .WithContent(content)
                .WithUrl(newUrl)
                .ExecuteAsync<ContentResponse<List<T>>>();
            return (result?.Value?.Data ?? new List<T>()).Where(w => w != null).ToList();
        }

        public List<T> PostList<T>(string path, object content) where T : class
        {
            var newUrl = path.Replace(Client?.BaseAddress?.ToString() ?? "", "");
            var result = ApiRequestBuilder.New()
                .WithMethod(HttpMethod.Post)
                .WithContent(content)
                .WithUrl(newUrl)
                .ExecuteAsync<ContentResponse<List<T>>>()
                .GetAwaiter()
                .GetResult();
            return (result?.Value?.Data ?? new List<T>()).Where(w => w != null).ToList();
        }

        #endregion

        #region Private Methods

        private static T GetData<T>(ApiResponse<ContentResponse<T>> result) where T : class, new()
        {
            var data = result?.Value?.Data;
            if (data is null && typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
                return new T();

            return data;
        }

        private static T GetData<T>(ApiResponse<T> result) where T : class, new()
        {
            var data = result?.Value;
            if (data is null && typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
                return new T();

            return data;
        }

        #endregion
    }
}