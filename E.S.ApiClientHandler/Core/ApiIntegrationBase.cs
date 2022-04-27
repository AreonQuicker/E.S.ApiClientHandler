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
    public abstract class ApiIntegrationBase : IApiIntegration
    {
        protected ApiIntegrationBase(HttpClient client)
        {
            Client = client;

            ApiRequestBuilder = Core.ApiRequestBuilder.Make(Client,
                ApiRequestBuilderConfig.Create(true, ApiRequestBuilderLoggerConfig.Create(null, null)));
        }

        protected ApiIntegrationBase(HttpClient client, IApiCachingManager cachingManager,
            int absoluteExpirationRelativeToNowInSeconds)
        {
            Client = client;

            ApiRequestBuilder = Core.ApiRequestBuilder.Make(Client,
                ApiRequestBuilderConfig.Create(true, ApiRequestBuilderLoggerConfig.Create(null, null), cachingManager,
                    absoluteExpirationRelativeToNowInSeconds));
        }

        protected ApiIntegrationBase(HttpClient client, string user, ILogger logger,
            IApiCachingManager cachingManager,
            int absoluteExpirationRelativeToNowInSeconds)
        {
            Client = client;

            ApiRequestBuilder = Core.ApiRequestBuilder.Make(Client,
                ApiRequestBuilderConfig.Create(true, ApiRequestBuilderLoggerConfig.Create(logger, user), cachingManager,
                    absoluteExpirationRelativeToNowInSeconds));
        }

        protected ApiIntegrationBase(string apiBaseUrl, string authorizationHeader)
        {
            if (apiBaseUrl == null) throw new ArgumentNullException(nameof(apiBaseUrl));

            Client = new HttpClient {BaseAddress = new Uri(apiBaseUrl)};

            if (!string.IsNullOrEmpty(authorizationHeader))
                Client.DefaultRequestHeaders.Add("Authorization", authorizationHeader);

            ApiRequestBuilder = Core.ApiRequestBuilder.Make(Client, apiBaseUrl,
                ApiRequestBuilderConfig.Create(true, ApiRequestBuilderLoggerConfig.Create(null, null)));
        }

        protected ApiIntegrationBase(string apiBaseUrl, string authorizationHeader,
            IApiCachingManager cachingManager,
            int absoluteExpirationRelativeToNowInSeconds)
        {
            if (apiBaseUrl == null) throw new ArgumentNullException(nameof(apiBaseUrl));

            Client = new HttpClient {BaseAddress = new Uri(apiBaseUrl)};

            if (!string.IsNullOrEmpty(authorizationHeader))
                Client.DefaultRequestHeaders.Add("Authorization", authorizationHeader);

            ApiRequestBuilder = Core.ApiRequestBuilder.Make(Client, apiBaseUrl,
                ApiRequestBuilderConfig.Create(true, ApiRequestBuilderLoggerConfig.Create(null, null), cachingManager,
                    absoluteExpirationRelativeToNowInSeconds));
        }

        protected ApiIntegrationBase(string apiBaseUrl, string authorizationHeader,
            string user, ILogger logger,
            IApiCachingManager cachingManager,
            int absoluteExpirationRelativeToNowInSeconds)
        {
            if (apiBaseUrl == null) throw new ArgumentNullException(nameof(apiBaseUrl));

            Client = new HttpClient {BaseAddress = new Uri(apiBaseUrl)};

            if (!string.IsNullOrEmpty(authorizationHeader))
                Client.DefaultRequestHeaders.Add("Authorization", authorizationHeader);

            ApiRequestBuilder = Core.ApiRequestBuilder.Make(Client, apiBaseUrl,
                ApiRequestBuilderConfig.Create(true, ApiRequestBuilderLoggerConfig.Create(logger, user), cachingManager,
                    absoluteExpirationRelativeToNowInSeconds));
        }

        public IApiRequestBuilder ApiRequestBuilder { get; }
        public HttpClient Client { get; }

        #region IDisposable

        public void Dispose()
        {
            ApiRequestBuilder.Dispose();
        }

        #endregion

        #region IIntegrationBase

        public async Task<T> GetAsync<T>(string path, bool withCache = false) where T : class, new()
        {
            var newUrl = path.Replace(Client?.BaseAddress?.ToString() ?? "", "");
            var result = await ApiRequestBuilder.New()
                .WithMethod(HttpMethod.Get)
                .WithUrl(newUrl)
                .ExecuteAsync<ContentResponse<T>>();
            return GetData(result);
        }

        public T Get<T>(string path, bool withCache = false) where T : class, new()
        {
            return GetAsync<T>(path, withCache).GetAwaiter().GetResult();
        }

        public async Task<T> GetAsIsAsync<T>(string path, bool withCache = false) where T : class, new()
        {
            var newUrl = path.Replace(Client?.BaseAddress?.ToString() ?? "", "");
            var result = await ApiRequestBuilder.New()
                .WithMethod(HttpMethod.Get)
                .WithUrl(newUrl)
                .ExecuteAsync<T>();
            return GetData(result);
        }

        public T GetAsIs<T>(string path, bool withCache = false) where T : class, new()
        {
            return GetAsIsAsync<T>(path, withCache).GetAwaiter().GetResult();
        }

        public async Task<List<T>> GetListAsync<T>(string path, bool withCache = false) where T : class
        {
            var newUrl = path.Replace(Client?.BaseAddress?.ToString() ?? "", "");
            var result = await ApiRequestBuilder.New()
                .WithMethod(HttpMethod.Get)
                .WithUrl(newUrl)
                .ExecuteAsync<ContentResponse<List<T>>>();

            return (result?.Value?.Data ?? new List<T>()).Where(w => w != null).ToList();
        }

        public List<T> GetList<T>(string path, bool withCache = false) where T : class
        {
            return GetListAsync<T>(path, withCache).GetAwaiter().GetResult();
        }

        public async Task<List<T>> GetAsIsListAsync<T>(string path, bool withCache = false) where T : class
        {
            var newUrl = path.Replace(Client?.BaseAddress?.ToString() ?? "", "");
            var result = await ApiRequestBuilder.New()
                .WithMethod(HttpMethod.Get)
                .WithUrl(newUrl)
                .ExecuteAsync<List<T>>();
            return (result?.Value ?? new List<T>()).Where(w => w != null).ToList();
        }

        public List<T> GetAsIsList<T>(string path, bool withCache = false) where T : class
        {
            return GetAsIsListAsync<T>(path, withCache).GetAwaiter().GetResult();
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
            return PostAsync<T>(path, content).GetAwaiter().GetResult();
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
            return PostAsIsAsync<T>(path, content).GetAwaiter().GetResult();
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
            return PostListAsync<T>(path, content).GetAwaiter().GetResult();
        }

        public void SetUser(string user)
        {
            ApiRequestBuilder.SetUser(user);
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