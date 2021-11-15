using E.S.ApiClientHandler.Models;
using E.S.ApiClientHandler.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace E.S.ApiClientHandler.Interfaces
{
    public interface IApiRequestBuilder : IDisposable
    {
        IApiRequestBuilder WithHttpRequestMessage(HttpRequestMessageWrapper httpRequestMessageWrapper);
        IApiRequestBuilder WithUrl(ApiUrlBuilder requestUrlBuilder);
        IApiRequestBuilder WithUrl(string url);
        IApiRequestBuilder AddHeader(string key, string header);
        IApiRequestBuilder AddKeyAndHeaders(Dictionary<string, string> keyAndHeaders);
        IApiRequestBuilder WithContent(object content);
        IApiRequestBuilder WithMethod(HttpMethod httpMethod);
        IApiRequestBuilder WithCacheClient(IApiCachingManager apiCachingManager, int absoluteExpirationRelativeToNowInSeconds = 300);
        Task<ApiResponse<T>> ExecuteAsync<T>(T defaultValue = null) where T : class, new();
        Task<ApiResponse> ExecuteNoValueAsync();
        IApiRequestBuilder New();
    }
}
