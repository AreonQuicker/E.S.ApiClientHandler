using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using E.S.ApiClientHandler.Models;
using E.S.ApiClientHandler.Utils;

namespace E.S.ApiClientHandler.Interfaces
{
    public interface IApiRequestBuilder : IDisposable
    {
        IApiRequestBuilder WithCacheClient(IApiCachingManager apiCachingManager,
            int absoluteExpirationRelativeToNowInSeconds);

        IApiRequestBuilderInner1 New();
    }

    public interface IApiRequestBuilderInner1 : IDisposable
    {
        IApiRequestBuilderInner1 WithHttpRequestMessage(HttpRequestMessageWrapper httpRequestMessageWrapper);
        IApiRequestBuilderInner1 AddHeader(string key, string header);
        IApiRequestBuilderInner1 AddKeyAndHeaders(Dictionary<string, string> keyAndHeaders);
        IApiRequestBuilderInner1 WithContent(object content);
        IApiRequestBuilderInner1 WithMethod(HttpMethod httpMethod);
        
        IApiRequestBuilderInner1 WithCache(bool withCache = false);
        IApiRequestBuilderInner2 WithUrl(ApiUrlBuilder requestUrlBuilder);
        IApiRequestBuilderInner2 WithUrl(string url);
    }

    public interface IApiRequestBuilderInner2 : IDisposable
    {
        Task<ApiResponse<T>> ExecuteAsync<T>(T defaultValue = null) where T : class, new();
        Task<ApiResponse> ExecuteNoValueAsync();
        Task<ApiResponse<string>> ExecuteAsync();
    }
}